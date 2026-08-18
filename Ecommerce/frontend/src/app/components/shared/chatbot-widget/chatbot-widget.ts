import { Component, inject, signal, ViewChild, ElementRef, OnInit, AfterViewChecked, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { LLMService, ChatMessageDTO } from '../../../services/llm.service';
import { AuthService } from '../../../services/auth.service';
import { resolveImageUrl } from '../../../utils/image.util';

export interface InlineSegment {
  type: 'text' | 'link';
  text: string;
  url?: string;
  isActionable?: boolean;
}

export interface ChatBlock {
  type: 'text' | 'table' | 'variant-card' | 'order-card';
  text?: string;
  inlineSegments?: InlineSegment[];
  table?: {
    headers: string[];
    rows: string[][];
  };
  variant?: {
    imageUrl: string;
    title: string;
    details: string;
    productId?: number;
  };
  orderCard?: {
    orderId: string;
    total: string;
    status: string;
    items?: string;
    trackingInfo?: string;
  };
}

@Component({
  selector: 'app-chatbot-widget',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chatbot-widget.html',
  styleUrl: './chatbot-widget.css',
})
export class ChatbotWidget implements OnInit, AfterViewChecked {
  private chatbotService = inject(LLMService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private elementRef = inject(ElementRef);

  @ViewChild('scrollContainer') private scrollContainer!: ElementRef;

  isOpen = signal(false);
  isLoading = signal(false);
  newMessage = signal('');
  messages = signal<ChatMessageDTO[]>([]);
  role = signal<string>('');
  isLoggedIn = signal<boolean>(false);
  showChatbot = signal(true);

  ngOnInit(): void {
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.checkRoute();
    });
    this.checkRoute();

    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.isLoggedIn.set(true);
        this.role.set(user.role || '');
        this.loadHistory();
      } else {
        this.isLoggedIn.set(false);
        this.role.set('Guest');
        this.messages.set([]);
      }
    });
  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  toggleChat(): void {
    this.isOpen.update(val => !val);
    if (this.isOpen()) {
      setTimeout(() => this.scrollToBottom(), 50);
    }
  }

  checkRoute(): void {
    const url = this.router.url.toLowerCase();
    const hideOnPaths = ['login', 'register'];
    const shouldHide = hideOnPaths.some(path => url.includes(path));
    this.showChatbot.set(!shouldHide);
    if (shouldHide && this.isOpen()) {
      this.isOpen.set(false);
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.isOpen()) return;

    const clickedInside = this.elementRef.nativeElement.contains(event.target);
    if (!clickedInside) {
      this.isOpen.set(false);
    }
  }

  getSuggestions(): string[] {
    const r = this.role().toLowerCase();
    if (r === 'admin') {
      return [
        'Show platform KPI statistics',
        'Show overall dashboard sales figures',
        'List registered vendors and their turnovers'
      ];
    } else if (r === 'vendor') {
      return [
        'Check inventory stock alerts',
        'Show my settlements and payout status'
      ];
    } else { // Customer or Guest
      return [
        'Search for laptops',
        'Show my cart',
        'Show my wishlist',
        'Track my orders'
      ];
    }
  }

  selectSuggestion(suggestion: string): void {
    this.newMessage.set(suggestion);
    this.sendMessage();
  }

  loadHistory(): void {
    this.chatbotService.getChatHistory().subscribe({
      next: (history) => {
        if (history && history.messages) {
          this.messages.set(history.messages);
        }
      },
      error: (err) => console.error('Failed to load chat history:', err)
    });
  }

  sendMessage(): void {
    const text = this.newMessage().trim();
    if (!text || this.isLoading()) return;

    const userMsg: ChatMessageDTO = {
      sender: 'User',
      content: text,
      createdAt: new Date()
    };

    this.messages.update(msgs => [...msgs, userMsg]);
    this.newMessage.set('');
    this.isLoading.set(true);
    setTimeout(() => this.scrollToBottom(), 10);

    this.chatbotService.sendMessage(text).subscribe({
      next: (res) => {
        const aiMsg: ChatMessageDTO = {
          sender: 'AI',
          content: res.reply,
          createdAt: new Date()
        };
        this.messages.update(msgs => [...msgs, aiMsg]);
        this.isLoading.set(false);
        setTimeout(() => this.scrollToBottom(), 10);
      },
      error: (err) => {
        console.error('Chat error:', err);
        const errorMsg: ChatMessageDTO = {
          sender: 'AI',
          content: 'An error occurred while connecting to the chat assistant. Please make sure the Gemini API Key is configured in appsettings.json.',
          createdAt: new Date()
        };
        this.messages.update(msgs => [...msgs, errorMsg]);
        this.isLoading.set(false);
        setTimeout(() => this.scrollToBottom(), 10);
      }
    });
  }

  clearChat(): void {
    if (confirm('Are you sure you want to clear your conversation history?')) {
      this.chatbotService.clearChat().subscribe({
        next: () => {
          this.messages.set([]);
        },
        error: (err) => console.error('Failed to clear chat:', err)
      });
    }
  }

  private scrollToBottom(): void {
    if (this.scrollContainer) {
      try {
        const el = this.scrollContainer.nativeElement;
        el.scrollTop = el.scrollHeight;
      } catch (err) {}
    }
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  // Content parsing functions
  parseMessage(content: string): ChatBlock[] {
    if (!content) return [];
    // Convert any dollar sign ($) to Rupee sign (₹) throughout the message content
    content = content.replace(/\$/g, '₹');
    const blocks: ChatBlock[] = [];
    const lines = content.split('\n');
    let currentTableLines: string[] = [];
    let inTable = false;

    for (let i = 0; i < lines.length; i++) {
      const line = lines[i].trim();
      const isTableLine = line.startsWith('|') && line.endsWith('|');
      
      if (isTableLine) {
        inTable = true;
        currentTableLines.push(line);
      } else {
        if (inTable && currentTableLines.length > 0) {
          const tableBlock = this.parseTableBlock(currentTableLines);
          if (tableBlock) {
            blocks.push(tableBlock);
          } else {
            blocks.push({ 
              type: 'text', 
              text: currentTableLines.join('\n'),
              inlineSegments: this.parseInlineSegments(currentTableLines.join('\n'))
            });
          }
          currentTableLines = [];
          inTable = false;
        }
        
        if (line) {
          const imageRegex = /!\[([^\]]*)\]\(([^)]+)\)/;
          const match = line.match(imageRegex);
          if (match) {
            const imageUrl = match[2];
            const altText = match[1];
            const details = line.replace(imageRegex, '').trim();
            const productId = this.extractProductId(details) || this.extractProductId(imageUrl);
            
            blocks.push({
              type: 'variant-card',
              variant: {
                imageUrl: this.resolveImage(imageUrl),
                title: altText || 'Product Variant',
                details: details,
                productId: productId || undefined
              }
            });
          } else {
            // Check if this line is an order summary line
            const orderIdMatch = line.match(/(?:order)\s*(?:id|#)?\s*:?\s*(\d+)/i);
            const hasTotalOrStatus = line.toLowerCase().includes('total') || line.toLowerCase().includes('status');
            
            if (orderIdMatch && hasTotalOrStatus) {
              const orderId = orderIdMatch[1];
              let total = '';
              let status = '';
              
              const totalMatch = line.match(/total:?\s*([^,\n]+)/i);
              if (totalMatch) {
                total = totalMatch[1].trim().replace(/\.*$/, '').trim();
                total = total.replace(/\$/g, '₹');
                if (total && !total.startsWith('₹')) {
                  const numVal = parseFloat(total.replace(/[^0-9.]/g, ''));
                  if (!isNaN(numVal)) {
                    total = '₹' + total;
                  }
                }
              }
              
              const statusMatch = line.match(/status:?\s*([^,\n\.]+)/i);
              if (statusMatch) {
                status = statusMatch[1].trim();
              }
              
              // Let's check if the next line lists items
              let items = '';
              if (i + 1 < lines.length) {
                const nextLine = lines[i + 1].trim();
                if (nextLine.toLowerCase().startsWith('* items:') || nextLine.toLowerCase().startsWith('- items:') || nextLine.toLowerCase().startsWith('items:')) {
                  items = nextLine.replace(/^(\*|-)?\s*items:\s*/i, '').trim();
                  i++; // consume it
                }
              }
              
              // Let's check if there is tracking/shipment details on the next line
              let trackingInfo = '';
              if (i + 1 < lines.length) {
                const nextLine = lines[i + 1].trim();
                const nextLineLower = nextLine.toLowerCase();
                const isAnotherOrder = nextLine.match(/(?:order)\s*(?:id|#)?\s*:?\s*(\d+)/i) && (nextLineLower.includes('total') || nextLineLower.includes('status'));
                
                if (!isAnotherOrder && (nextLineLower.includes('track') || nextLineLower.includes('shipment') || nextLineLower.includes('shipped') || nextLineLower.includes('carrier') || nextLineLower.includes('delivered'))) {
                  trackingInfo = nextLine.replace(/^(\*|-)\s*/, '').trim();
                  i++; // consume it
                }
              }
              
              blocks.push({
                type: 'order-card',
                orderCard: {
                  orderId,
                  total,
                  status,
                  items,
                  trackingInfo
                }
              });
            } else {
              blocks.push({ 
                type: 'text', 
                text: line,
                inlineSegments: this.parseInlineSegments(line)
              });
            }
          }
        }
      }
    }

    if (inTable && currentTableLines.length > 0) {
      const tableBlock = this.parseTableBlock(currentTableLines);
      if (tableBlock) {
        blocks.push(tableBlock);
      } else {
        blocks.push({ 
          type: 'text', 
          text: currentTableLines.join('\n'),
          inlineSegments: this.parseInlineSegments(currentTableLines.join('\n'))
        });
      }
    }

    return blocks;
  }

  resolveImage(url: string): string {
    return resolveImageUrl(url);
  }

  private parseTableBlock(lines: string[]): ChatBlock | null {
    if (lines.length < 2) return null;
    
    const headers = lines[0]
      .split('|')
      .map(cell => cell.trim())
      .filter((_, idx, arr) => idx > 0 && idx < arr.length - 1);
      
    const rows: string[][] = [];
    for (let i = 2; i < lines.length; i++) {
      const rowCells = lines[i]
        .split('|')
        .map(cell => cell.trim())
        .filter((_, idx, arr) => idx > 0 && idx < arr.length - 1);
      if (rowCells.length > 0) {
        rows.push(rowCells);
      }
    }
    
    return {
      type: 'table',
      table: { headers, rows }
    };
  }

  parseInlineSegments(text: string): InlineSegment[] {
    const initialSegments: InlineSegment[] = [];
    const regex = /\[([^\]]+)\]\(([^)]+)\)/g;
    let lastIndex = 0;
    let match;

    while ((match = regex.exec(text)) !== null) {
      const textBefore = text.substring(lastIndex, match.index);
      if (textBefore) {
        initialSegments.push({ type: 'text', text: textBefore });
      }
      
      const linkText = match[1];
      const linkUrl = match[2];
      initialSegments.push({
        type: 'link',
        text: linkText,
        url: linkUrl,
        isActionable: this.isCustomScheme(linkUrl)
      });
      
      lastIndex = regex.lastIndex;
    }

    const textAfter = text.substring(lastIndex);
    if (textAfter) {
      initialSegments.push({ type: 'text', text: textAfter });
    }

    const finalSegments: InlineSegment[] = [];
    for (const seg of initialSegments) {
      if (seg.type === 'link') {
        finalSegments.push(seg);
      } else {
        finalSegments.push(...this.parseRawKeywords(seg.text));
      }
    }
    return finalSegments;
  }

  private isCustomScheme(url: string): boolean {
    return url.startsWith('product:') || 
           url.startsWith('order:') || 
           url === 'wishlist' || 
           url === 'cart' || 
           url === 'orders' || 
           url === 'products' || 
           url === 'settlements' || 
           url.startsWith('settlement:');
  }

  private parseRawKeywords(text: string): InlineSegment[] {
    const regex = /\b(product-variant|product|variant)\s*(?:id|#)?\s*:?\s*(\d+)\b|\b(order)\s*(?:id|#)?\s*:?\s*(\d+)\b|\b(wishlist|cart|settlements?|products)\b/gi;
    
    const segments: InlineSegment[] = [];
    let lastIndex = 0;
    let match;
    
    while ((match = regex.exec(text)) !== null) {
      const textBefore = text.substring(lastIndex, match.index);
      if (textBefore) {
        segments.push({ type: 'text', text: textBefore });
      }
      
      const matchedText = match[0];
      let url = '';
      
      if (match[1] && match[2]) {
        url = `product:${match[2]}`;
      } else if (match[3] && match[4]) {
        url = `order:${match[4]}`;
      } else if (match[5]) {
        const keyword = match[5].toLowerCase();
        if (keyword.startsWith('settlement')) {
          url = 'settlements';
        } else if (keyword === 'products') {
          url = 'products';
        } else {
          url = keyword;
        }
      }
      
      segments.push({
        type: 'link',
        text: matchedText,
        url: url,
        isActionable: true
      });
      
      lastIndex = regex.lastIndex;
    }
    
    const textAfter = text.substring(lastIndex);
    if (textAfter) {
      segments.push({ type: 'text', text: textAfter });
    }
    
    return segments;
  }

  private extractProductId(text: string): number | null {
    const patterns = [
      /product:(\d+)/i,
      /product\/(\d+)/i,
      /product-detail\/(\d+)/i,
      /variant:(\d+)/i,
      /id:?\s*(\d+)/i,
      /product\s*(\d+)/i,
      /variant\s*(\d+)/i
    ];
    
    for (const pattern of patterns) {
      const match = pattern.exec(text);
      if (match) {
        const id = parseInt(match[1], 10);
        if (!isNaN(id)) return id;
      }
    }
    return null;
  }

  handleLinkClick(url: string): void {
    const currentRole = this.role();
    
    if (url.startsWith('product:')) {
      const id = url.split(':')[1];
      if (currentRole === 'Admin') {
        this.router.navigate([`/admin-home/product-detail/${id}`]);
      } else if (currentRole === 'Vendor') {
        this.router.navigate([`/vendor-home/product-detail/${id}`]);
      } else {
        this.router.navigate([`/customer-home/product-detail/${id}`]);
      }
      this.isOpen.set(false);
    } else if (url.startsWith('order:')) {
      const id = url.split(':')[1];
      if (currentRole === 'Admin') {
        this.router.navigate([`/admin-home/order-detail/${id}`]);
      } else if (currentRole === 'Vendor') {
        this.router.navigate([`/vendor-home/order-detail/${id}`]);
      } else {
        this.router.navigate([`/customer-home/order-detail/${id}`]);
      }
      this.isOpen.set(false);
    } else if (url === 'orders') {
      if (currentRole === 'Admin') {
        this.router.navigate(['/admin-home/order-list']);
      } else if (currentRole === 'Vendor') {
        this.router.navigate(['/vendor-home/order-list']);
      } else {
        this.router.navigate(['/customer-home/order-list']);
      }
      this.isOpen.set(false);
    } else if (url === 'wishlist') {
      if (currentRole === 'Customer') {
        this.router.navigate(['/customer-home/wishlist']);
        this.isOpen.set(false);
      }
    } else if (url === 'cart') {
      if (currentRole === 'Customer') {
        this.router.navigate(['/customer-home/cart']);
        this.isOpen.set(false);
      }
    } else if (url === 'products') {
      if (currentRole === 'Vendor') {
        this.router.navigate(['/vendor-home/products-list']);
      } else if (currentRole === 'Admin') {
        this.router.navigate(['/admin-home/products-list']);
      } else {
        this.router.navigate(['/customer-home/products-list']);
      }
      this.isOpen.set(false);
    } else if (url === 'settlements' || url.startsWith('settlement:')) {
      if (currentRole === 'Vendor') {
        this.router.navigate(['/vendor-home/settlements']);
        this.isOpen.set(false);
      }
    }
  }

  isComparison(headers: string[], content: string): boolean {
    const isComparisonText = /compare|comparison|versus|\bvs\b/i.test(content || '');
    const hasAttributeHeader = headers.some(h => /feature|attribute|specification|property|field|kpi|metric/i.test(h));
    return isComparisonText || (headers.length >= 3 && hasAttributeHeader);
  }

  getStatusClass(status: string): string {
    if (!status) return 'status-unknown';
    const s = status.toLowerCase();
    if (s.includes('delivered')) return 'status-delivered';
    if (s.includes('shipped') || s.includes('transit')) return 'status-shipped';
    if (s.includes('pending') || s.includes('processing') || s.includes('placed')) return 'status-pending';
    if (s.includes('cancel')) return 'status-cancelled';
    return 'status-unknown';
  }
}
