import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

export interface ChatMessageDTO {
  id?: number;
  sender: 'User' | 'AI';
  content: string;
  createdAt: Date;
}

export interface ChatSessionDTO {
  id: number;
  userId: number;
  role: string;
  createdAt: Date;
  messages: ChatMessageDTO[];
}

export interface ChatMessageResponse {
  reply: string;
  sessionId: number;
}

@Injectable({
  providedIn: 'root'
})
export class LLMService {
  private baseUrl = `${environment.baseUrl}/Chatbot`;

  constructor(private http: HttpClient) {}

  sendMessage(message: string): Observable<ChatMessageResponse> {
    return this.http.post<ChatMessageResponse>(`${this.baseUrl}/message`, { message });
  }

  getChatHistory(): Observable<ChatSessionDTO> {
    return this.http.get<ChatSessionDTO>(`${this.baseUrl}/history`);
  }

  clearChat(): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/clear`, {});
  }

  generateSpecs(productName: string, productDescription: string, specDescription: string): Observable<{ [key: string]: string }> {
    return this.http.post<{ [key: string]: string }>(`${this.baseUrl}/generate-specs`, {
      productName,
      productDescription,
      specDescription
    });
  }
}
