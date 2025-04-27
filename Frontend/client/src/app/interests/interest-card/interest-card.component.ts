import { Component, Input, OnInit } from '@angular/core';
import { Interest } from '../../_models/Interest';
import { images } from '../../constants/interest-resources';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-interest-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './interest-card.component.html',
  styleUrl: './interest-card.component.css'
})
export class InterestCardComponent implements OnInit{
  @Input() interests!: Interest[];
  images = images;

  ngOnInit(): void {
    console.log(this.interests);
  }

}
