import { Directive, ElementRef, EventEmitter, HostListener, Input, Output } from '@angular/core';

@Directive({
  selector: '[appClickOutside]',
  standalone: true
})
export class ClickOutsideDirective {
  @Output() appClickOutside = new EventEmitter<void>();
  @Input('appClickOutsideExcludeSelectors') excludeSelectors: string[] = [];

  constructor(private elementRef: ElementRef) {}

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    const clickedInside = this.elementRef.nativeElement.contains(target);
    const clickedExcluded = this.excludeSelectors.some(sel =>
      target.closest(sel) !== null
    );
    if (!clickedInside && !clickedExcluded) {
      this.appClickOutside.emit();
    }
  }
}
