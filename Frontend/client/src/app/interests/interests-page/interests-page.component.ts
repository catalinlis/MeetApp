import { Component, inject, OnInit } from '@angular/core';
import { NavbarComponent } from "../../navbar/navbar.component";
import { ProfileIdentityComponent } from "../../profile/profile-identity/profile-identity.component";
import { InterestService } from '../../_services/interest.service';
import { Interest } from '../../_models/Interest';
import { InterestCardComponent } from "../interest-card/interest-card.component";

@Component({
  selector: 'app-interests-page',
  standalone: true,
  imports: [NavbarComponent, ProfileIdentityComponent, InterestCardComponent],
  templateUrl: './interests-page.component.html',
  styleUrl: './interests-page.component.css'
})
export class InterestsPageComponent implements OnInit{
  private interestsService = inject(InterestService);
  interests: Interest[] = [];

  ngOnInit(): void {
    this.interestsService.getInterests().subscribe((interests) => {
      this.interests = interests;
    })
  }
}
