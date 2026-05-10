import {
  Component, EventEmitter, Input, Output,
  AfterViewInit, OnChanges, SimpleChanges,
  ElementRef, ViewChild
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import {CdkDragHandle} from '@angular/cdk/drag-drop';
import {MatIconModule} from '@angular/material/icon';

@Component({
  selector: 'app-html-block',
  imports: [FormsModule, CdkDragHandle, MatIconModule],
  templateUrl: './html-block.html',
  styleUrl: './html-block.css',
  standalone: true
})
export class HtmlBlock implements AfterViewInit, OnChanges {
  @ViewChild('ta') taRef!: ElementRef<HTMLTextAreaElement>;

  @Input() id!: number;
  @Input() text!: string;
  @Input() name: string = 'block';

  @Output() focused = new EventEmitter<void>();
  @Output() blurred = new EventEmitter<void>();
  @Output() textChange = new EventEmitter<string>();
  @Output() nameChange = new EventEmitter<string>();

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

  onNameInput(event: Event) {
    this.nameChange.emit((event.target as HTMLInputElement).value);
  }

  onFocus() { this.focused.emit(); }
  onBlur() { this.blurred.emit(); }
}
