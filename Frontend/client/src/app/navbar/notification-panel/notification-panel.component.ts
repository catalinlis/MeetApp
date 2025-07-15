import { Component, inject, OnInit } from '@angular/core';
import { LoaderComponent } from "../../loader/loader.component";
import { NotificationService } from '../../_services/notification.service';
import { RealtimeNotification } from '../../_models/RealtimeNotification';
import { CommonModule } from '@angular/common';
import { AccountService } from '../../_services/account.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { DateProcessing } from '../../utils/DateProcessing';
import { faCircle } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

interface ExtendedRealtimeNotification{
  realtimeNotification: RealtimeNotification,
  image: SafeUrl | null,
  createdAt: string
}

@Component({
  selector: 'app-notification-panel',
  standalone: true,
  imports: [LoaderComponent, CommonModule, FontAwesomeModule],
  templateUrl: './notification-panel.component.html',
  styleUrl: './notification-panel.component.css'
})
export class NotificationPanelComponent implements OnInit{
  faCircle=faCircle;
  private notificationService = inject(NotificationService);
  private accountService = inject(AccountService);
  private sanitizer = inject(DomSanitizer);
  notifications: ExtendedRealtimeNotification[] = [];
  loadedPhotos: boolean = false;
  noNotifications: boolean = false;

  ngOnInit(): void {
    this.markAsSeenAllNotifications().then(_ => {
      this.getNotifications().then((notifications) => {
        this.noNotifications = notifications.length === 0 ? true : false;

        const uniqueProfilePhotos = [...new Set(notifications.map(notification => notification.senderProfilePhoto))];
        const imgPromises = uniqueProfilePhotos.map((profilePhoto) => {
          return this.getProfilePhoto(profilePhoto).then((img) => ({profilePhoto: profilePhoto, image: img}))
            .catch(_ => ({profilePhoto: profilePhoto, image: null}));
        });

        Promise.all(imgPromises).then(results => {
          var images = results.filter(({image}) => image !== null);

          notifications.forEach((notification) => {
            const match = images.find(({profilePhoto}) => profilePhoto == notification.senderProfilePhoto);
            const image = match ? match.image : null;

            const pushNotification = {
              realtimeNotification: notification,
              image: image,
              createdAt: DateProcessing.formatPostDate(notification.createdAt)
            };

            this.notifications.push(pushNotification);
          });

          this.loadedPhotos = true;
        });
      });
    });
  }

  getNotifications(): Promise<RealtimeNotification[]>{
    
    return new Promise<RealtimeNotification[]>((resolve, reject) => {
      this.notificationService.getRealtimeNotifications().subscribe({
        next: (response) => {
          resolve(response);
        },
        error: (err) => {
          console.log(err.error);
          reject();
        }
      });
    });
  }

  markAsSeenAllNotifications(): Promise<void>{
    return new Promise<void>((resolve,reject) => {
      this.notificationService.markAsSeenAllNotifications().subscribe({
        next: (response) => {
          resolve();
        },
        error: (err) => {
          console.log(err.error);
          reject();
        }
      })
    })
  }

  async getProfilePhoto(photoId: string): Promise<SafeUrl>{
    return new Promise((resolve, reject) => {
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
}
