import { Component, computed, inject, Input, OnInit, signal } from '@angular/core';
import { AboutMember } from '../../../../_models/AboutMember';
import { MONTHS } from '../../../../constants/data-constants';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faBirthdayCake, faM, IconDefinition } from '@fortawesome/free-solid-svg-icons';
import { faMailBulk } from '@fortawesome/free-solid-svg-icons';
import { faFemale } from '@fortawesome/free-solid-svg-icons';
import { faMale } from '@fortawesome/free-solid-svg-icons';
import { CommonModule } from '@angular/common';
import { MembersService } from '../../../../_services/members.service';
import { Member } from '../../../../_models/Member';
import { User } from '../../../../_models/User';
import { StringProcess } from '../../../../utils/StringProcess';
import { LoaderComponent } from "../../../../loader/loader.component";

@Component({
  selector: 'app-member-about',
  standalone: true,
  imports: [FontAwesomeModule, CommonModule, LoaderComponent],
  templateUrl: './member-about.component.html',
  styleUrl: './member-about.component.css'
})
export class MemberAboutComponent implements OnInit{
  private memberService = inject(MembersService);
  faBirthdayCake = faBirthdayCake;
  faMailBulk = faMailBulk;
  faMale = faMale;
  faFemale = faFemale;
  Genders: Record<'Male' | 'Female', IconDefinition> = {
    Male: faMale,
    Female: faFemale
  };
  BirthDay = '';
  loaded: boolean = false;
  @Input() member!: Member;
  aboutMember: AboutMember | null = null;


  ngOnInit(): void {
    this.getAboutMember(this.member.username);
  }

  getBirthDay(birthday: Date): string{
    
    const dateBirthday = new Date(birthday);
    const date = String(dateBirthday.getDate());
    const month = MONTHS[dateBirthday.getMonth()] 
    const year = String(dateBirthday.getFullYear());
    
    return `${date} ${month} ${year}`;
  }

  getAboutMember(username: string) {
    this.memberService.getAboutMember(username).subscribe({
      next: (response) => {
        this.aboutMember = response;
        this.aboutMember.gender = StringProcess.capitalizeFirstLetter(this.aboutMember.gender);
        this.BirthDay = this.getBirthDay(this.aboutMember.dateOfBirth);
        this.loaded = true;
      },
      error: (err) => {
        console.log(err.error);
      }
    });
  }

}
