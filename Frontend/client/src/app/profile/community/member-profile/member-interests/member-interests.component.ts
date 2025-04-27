import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { images } from '../../../../constants/interest-resources';
import { RouterLink } from '@angular/router';
import { Interest } from '../../../../_models/Interest';

@Component({
  selector: 'app-member-interests',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './member-interests.component.html',
  styleUrl: './member-interests.component.css'
})
export class MemberInterestsComponent {
  @Input() interests: Interest[] = [];
  images = images;
}
