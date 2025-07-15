import { Component, Input } from '@angular/core';
import { RealtimeNotification } from '../_models/RealtimeNotification';
import { SafeUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-notification-toast',
  standalone: true,
  imports: [],
  templateUrl: './notification-toast.component.html',
  styleUrl: './notification-toast.component.css'
})
export class NotificationToastComponent{
  @Input() notification!: RealtimeNotification;
  @Input() image!: SafeUrl;
}
