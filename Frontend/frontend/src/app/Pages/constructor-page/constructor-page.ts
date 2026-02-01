import { Component } from '@angular/core';
import {NgForOf} from '@angular/common';
import {HtmlBlock} from '../../Components/html-block/html-block';

type Block = { id: number};

@Component({
  selector: 'app-constructor-page',
  imports: [
    NgForOf, HtmlBlock
  ],
  templateUrl: './constructor-page.html',
  styleUrl: './constructor-page.css',
})
export class ConstructorPage {
  blocks: Block[] = [];
  private nextId = 1;
  addBlock() {
    this.blocks.push({ id: this.nextId++ });
  }

  activeBlockId: number | null = null;

  deleteBlock() {
    if (this.activeBlockId === null) return;

    this.blocks = this.blocks.filter(
      b => b.id !== this.activeBlockId
    );

    this.activeBlockId = null;
  }

  trackById = (_: number, b: Block) => b.id;
}
