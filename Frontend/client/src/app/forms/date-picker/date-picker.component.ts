import { Component, ElementRef, input, Renderer2, Self } from '@angular/core';
import { ControlValueAccessor, ReactiveFormsModule, FormControl, NgControl } from '@angular/forms';
import { formatDate, NgIf } from '@angular/common';
import { BsDatepickerModule, BsDatepickerConfig } from 'ngx-bootstrap/datepicker';

@Component({
  selector: 'app-date-picker',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, BsDatepickerModule],
  templateUrl: './date-picker.component.html',
  styleUrl: './date-picker.component.css'
})
export class DatePickerComponent implements ControlValueAccessor{
  label = input<string>('');
  maxDate = input<Date>();
  bsConfig?: Partial<BsDatepickerConfig>;

  constructor(@Self() public ngControl: NgControl, private el: ElementRef, private renderer: Renderer2){
    this.ngControl.valueAccessor = this;
    this.bsConfig = {
      containerClass: 'theme-red',
      dateInputFormat: 'YYYY-MM-DD'
    }
  }
  

  writeValue(obj: any): void {}
  registerOnChange(fn: any): void {}
  registerOnTouched(fn: any): void {}
  setDisabledState?(isDisabled: boolean): void {}
  
  get control(){
    return this.ngControl.control as FormControl
  }

  ngAfterViewInit(): void {
    const input = this.el.nativeElement.querySelector('#datePicker');
    if (input) {
      this.renderer.listen(input, 'focus', (event: Event) => {
        event.preventDefault(); // Prevent the keyboard from appearing
        input.blur(); // Ensure the input loses focus immediately
      });
    }
  }
}
