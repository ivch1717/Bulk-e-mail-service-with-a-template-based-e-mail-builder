import {ChangeDetectorRef, Component, ElementRef, ViewChild} from '@angular/core';
import {NgForOf} from '@angular/common';
import {HtmlBlock} from '../../Components/html-block/html-block';

type Block = { id: number, html: string };

@Component({
  selector: 'app-constructor-page',
  imports: [
    HtmlBlock
  ],
  templateUrl: './constructor-page.html',
  styleUrl: './constructor-page.css',
})
export class ConstructorPage {
  blocks: Block[] = [];
  private nextId = 1;
  addBlock() {
    this.blocks.push({ id: this.nextId++, html: '' });
  }

  activeBlockId: number | null = null;

  deleteBlock() {
    if (this.activeBlockId === null) return;

    this.blocks = this.blocks.filter(
      b => b.id !== this.activeBlockId
    );

    this.activeBlockId = null;
  }

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  importBlock() {
    this.fileInput.nativeElement.click();
  }

  constructor(private cdr: ChangeDetectorRef) {}

  async onFileSelected(event?: Event) {
    if (!event) return;

    const input = event.target as HTMLInputElement | null;
    if (!input || !input.files?.length) return;

    const file = input.files[0];
    const text = await file.text();

    this.blocks = [...this.blocks, { id: this.nextId++, html: text }];

    this.cdr.detectChanges();

    input.value = '';
  }

  trackById = (_: number, b: Block) => b.id;
}
