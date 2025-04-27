import { Component, computed, Input, OnInit, signal } from '@angular/core';
import { AboutMember } from '../../../../_models/AboutMember';
import { MONTHS } from '../../../../constants/data-constants';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faBirthdayCake, faM, IconDefinition } from '@fortawesome/free-solid-svg-icons';
import { faMailBulk } from '@fortawesome/free-solid-svg-icons';
import { faFemale } from '@fortawesome/free-solid-svg-icons';
import { faMale } from '@fortawesome/free-solid-svg-icons';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-member-about',
  standalone: true,
  imports: [FontAwesomeModule, CommonModule],
  templateUrl: './member-about.component.html',
  styleUrl: './member-about.component.css'
})
export class MemberAboutComponent implements OnInit{
  faBirthdayCake = faBirthdayCake;
  faMailBulk = faMailBulk;
  faMale = faMale;
  faFemale = faFemale;
  Genders: Record<'Male' | 'Female', IconDefinition> = {
    Male: faMale,
    Female: faFemale
  };
  BirthDay = '';
  @Input() aboutMember!: AboutMember;

  ngOnInit(): void {
    this.BirthDay = this.getBirthDay(this.aboutMember.dateOfBirth);
  }

  getBirthDay(birthday: Date): string{
    
    const dateBirthday = new Date(birthday);
    const date = String(dateBirthday.getDate());
    const month = MONTHS[dateBirthday.getMonth()] 
    const year = String(dateBirthday.getFullYear());
    
    return `${date} ${month} ${year}`;
  }

}
