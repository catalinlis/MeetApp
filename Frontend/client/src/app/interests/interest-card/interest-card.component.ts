import { Component, Input, OnInit } from '@angular/core';
import { Interest } from '../../_models/Interest';
import { images } from '../../constants/interest-resources';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-interest-card',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './interest-card.component.html',
  styleUrl: './interest-card.component.css'
})
export class InterestCardComponent implements OnInit{
  @Input() interests!: Interest[];
  images = images;

  ngOnInit(): void {
  }

}
