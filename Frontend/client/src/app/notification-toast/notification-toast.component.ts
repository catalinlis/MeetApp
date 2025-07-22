import { Component, inject, Input } from '@angular/core';
import { RealtimeNotification } from '../_models/RealtimeNotification';
import { SafeUrl } from '@angular/platform-browser';
import { AccountService } from '../_services/account.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-notification-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-toast.component.html',
  styleUrl: './notification-toast.component.css'
})
export class NotificationToastComponent{
  public accountService = inject(AccountService);
  @Input() notification!: RealtimeNotification;
  @Input() image!: SafeUrl;
}
