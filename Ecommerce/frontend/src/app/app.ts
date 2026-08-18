import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Toast } from './components/shared/toast/toast';
import { ChatbotWidget } from './components/shared/chatbot-widget/chatbot-widget';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Toast, ChatbotWidget],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('frontend');
}
