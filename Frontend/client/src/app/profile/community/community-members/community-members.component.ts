import { Component, inject, OnInit } from '@angular/core';
import { NavbarComponent } from "../../../navbar/navbar.component";
import { ProfileIdentityComponent } from "../../profile-identity/profile-identity.component";
import { MemberCardComponent } from "../member-card/member-card.component";
import { MembersService } from '../../../_services/members.service';
import { Member } from '../../../_models/Member';

@Component({
  selector: 'app-community-members',
  standalone: true,
  imports: [NavbarComponent, ProfileIdentityComponent, MemberCardComponent],
  templateUrl: './community-members.component.html',
  styleUrl: './community-members.component.css'
})
export class CommunityMembersComponent implements OnInit {
  membersService = inject(MembersService);
  members : Member[] = [];

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers(){
    this.membersService.getMembers().subscribe({
      next: (members) => {
          this.members = members;
      },
      error: (error) => console.log(error.error)
    });
  }

}
