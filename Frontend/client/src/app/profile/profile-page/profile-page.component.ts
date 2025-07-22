import { Component} from '@angular/core';
import { ProfileIdentityComponent } from "../profile-identity/profile-identity.component";
import { FeedComponent } from "../feed/feed.component";
import { OnlineUsersComponent } from "../online-users/online-users.component";

@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [ ProfileIdentityComponent, FeedComponent, OnlineUsersComponent],
  templateUrl: './profile-page.component.html',
  styleUrl: './profile-page.component.css'
})
export class ProfilePageComponent{

}
