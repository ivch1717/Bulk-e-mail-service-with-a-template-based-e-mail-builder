import {
  Component, EventEmitter, Input, Output,
  AfterViewInit, OnChanges, SimpleChanges,
  ElementRef, ViewChild
} from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-html-block',
  imports: [FormsModule],
  templateUrl: './html-block.html',
  styleUrl: './html-block.css',
})
export class HtmlBlock implements AfterViewInit, OnChanges {
  @ViewChild('ta') taRef!: ElementRef<HTMLTextAreaElement>;

  @Input() id!: number;
  @Input() text!: string;

  @Output() focused = new EventEmitter<void>();
  @Output() blurred = new EventEmitter<void>();

  autoGrow(event?: Event) {
    const textarea = (event?.target as HTMLTextAreaElement) ?? this.taRef?.nativeElement;
    if (!textarea) return;

    textarea.style.height = 'auto';
    textarea.style.height = textarea.scrollHeight + 'px';
  }

  ngAfterViewInit() {
    queueMicrotask(() => this.autoGrow());
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['text'] && this.taRef) {
      queueMicrotask(() => this.autoGrow());
    }
  }

  onFocus() { this.focused.emit(); }
  onBlur() { this.blurred.emit(); }
}
