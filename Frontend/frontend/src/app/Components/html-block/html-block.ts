import {Component, EventEmitter, Input, Output} from '@angular/core';

@Component({
  selector: 'app-html-block',
  imports: [],
  templateUrl: './html-block.html',
  styleUrl: './html-block.css',
})
export class HtmlBlock {
  autoGrow(event: Event) {
    const textarea = event.target as HTMLTextAreaElement;

    textarea.style.height = 'auto';
    textarea.style.height = textarea.scrollHeight + 'px';
  }

  @Input() id!: number;
  @Output() focused = new EventEmitter<void>();
  @Output() blurred = new EventEmitter<void>();

  onFocus() {
    this.focused.emit();
  }

  onBlur() { this.blurred.emit(); }
}
