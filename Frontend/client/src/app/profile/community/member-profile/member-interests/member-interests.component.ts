import { Component, inject, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { images } from '../../../../constants/interest-resources';
import { RouterModule } from '@angular/router';
import { Interest } from '../../../../_models/Interest';
import { MembersService } from '../../../../_services/members.service';
import { Member } from '../../../../_models/Member';

@Component({
  selector: 'app-member-interests',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './member-interests.component.html',
  styleUrl: './member-interests.component.css'
})
export class MemberInterestsComponent implements OnInit{
  private memberService = inject(MembersService);
  @Input() member!: Member;
  interests: Interest[] = [];
  images = images;

  ngOnInit(): void {
    this.getMemberInterests(this.member.username);
  }

  getMemberInterests(username: string){
    this.memberService.getMemberInterests(username).subscribe({
      next: (response) => {
        this.interests = response;
      },
      error: (err) => {
        console.log(err.error);
      }
    })
  }
}
