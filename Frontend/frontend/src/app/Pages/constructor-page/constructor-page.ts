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

  exportBlock() {
    if (this.activeBlockId === null) return;

    console.log(this.blocks.filter(
      b => b.id == this.activeBlockId
    )[0].html);
  }

  exportTemplate(){
    let template: string = '';
    for (let i = 0; i < this.blocks.length; i++) {
      if (this.blocks[i].html.length > 0){
        template += this.blocks[i].html + '\n';
      }
    }
    if (template === '') {
      alert("Ошибка, шаблон пустой")
    }

    console.log(template);
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

  onBlockHtmlChange(id: string | number, html: string) {
    this.blocks = this.blocks.map(b =>
      b.id === id ? { ...b, html } : b
    );
  }

  trackById = (_: number, b: Block) => b.id;
}
