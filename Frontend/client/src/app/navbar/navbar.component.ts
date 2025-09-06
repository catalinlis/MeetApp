import { Component, inject, OnInit } from '@angular/core';
import { faDoorClosed, faMessage } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { AccountService } from '../_services/account.service';
import { Router } from '@angular/router';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { NotificationsHubService } from '../_services/hubs/notifications-hub.service';
import { RealtimeNotification } from '../_models/RealtimeNotification';
import { NotificationToastComponent } from '../notification-toast/notification-toast.component';
import { CommonModule } from '@angular/common';
import { NotificationPanelComponent } from "./notification-panel/notification-panel.component";
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { NotificationService } from '../_services/notification.service';
import { ClickOutsideDirective } from '../_directives/click-outside.directive';
import { filter } from 'rxjs';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [FontAwesomeModule, RouterModule, CommonModule, NotificationPanelComponent, ClickOutsideDirective],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit {
  faMessage = faMessage;
  faDoorClosed = faDoorClosed;
  private router = inject(Router);
  private sanitizer = inject(DomSanitizer);
  private toastr = inject(ToastrService);
  private notificationHubService = inject(NotificationsHubService);
  private notificationService = inject(NotificationService);
  accountService = inject(AccountService);
  notificationActive = false;
  notificationsCount = 0;

  ngOnInit(): void {
    this.accountService.userLoggedIn$
      .pipe(filter(user => !!user))
      .subscribe(user => {
        this.getNotificationsCount();
    });

    this.notificationHubService.receivedNotification$.subscribe(notification => {
      this.showNotificationToast(notification);
    });
  }

  private showNotificationToast(notification: RealtimeNotification){

    this.loadProfilePhoto(notification.senderProfilePhoto).then((image) => {
      this.notificationsCount++; 

      const toastRef = this.toastr.show('','',
        {
          toastComponent: NotificationToastComponent,
          positionClass: "toast-bottom-left",
          enableHtml: true,
          closeButton: true
        }
      );

      const instance = toastRef.toastRef.componentInstance as NotificationToastComponent
      instance.notification = notification;
      instance.image = image;
      
      setTimeout(() => {
        toastRef.toastRef.close();
      }, 10000);
    });
  }

  loadProfilePhoto(photoId: string): Promise<SafeUrl>{
    return new Promise<SafeUrl>((resolve, reject) => {
      this.accountService.getSignedUrl(photoId).subscribe({
        next: (response) => {
          const objectUrl = response.signedUrl;
          const img = new Image();
          img.src = objectUrl;
          img.onload = () => {
            const imageUrl = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
            resolve(imageUrl);
          }
        },
        error: (err) => {
          console.log(err.error);
          reject();
        }
      })
    })
  }

  getNotificationsCount(){
    this.notificationService.getNotificationsCount().subscribe({
      next: (response) => {
        this.notificationsCount = response;
      },
      error: (err) => {
        console.log(err.error);
      }
    })
  }

  toggleNotification(){
    this.notificationsCount = 0;
    this.notificationActive = !this.notificationActive;
  }

  handleCountClick(event: Event){
    event.stopPropagation();
    this.toggleNotification();
  }

  logout(): void{
    this.notificationActive = false;
    this.accountService.logout();
    this.router.navigateByUrl("/");
  }
}
