import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loader',
  standalone: true,
  imports: [ CommonModule ],
  templateUrl: './loader.component.html',
  styleUrl: './loader.component.css'
})
export class LoaderComponent {
  @Input() Cwidth: string = '';
  @Input() Cheight: string = '';
  @Input() MarginLeft: string = '';
  @Input() MarginTop: string = '';
  @Input() circle: boolean = false;
  @Input() inlineBlock: boolean = false;
  @Input() rgb: string = 'rgb(235, 235, 235)';
  @Input() radius: number = 4;

  getMyStyles(){
    const myStyles = {
      'width.px': this.Cwidth ? this.Cwidth : '',
      'height.px': this.Cheight ? this.Cheight: '',
      'margin-left.px': this.MarginLeft ? this.MarginLeft : '',
      'margin-top.px': this.MarginTop ? this.MarginTop : '',
      'display': this.inlineBlock ? 'inline-block' : '',
      'border-radius': this.circle ? '50%' : this.radius + 'px',
      'background-color': this.rgb
    }

    return myStyles;
  }
}
