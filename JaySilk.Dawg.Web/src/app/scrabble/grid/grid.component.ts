import { Component, OnInit, OnChanges, Input, EventEmitter, ViewChild, ElementRef, Output } from '@angular/core';
import { Bonus } from '../../models/rules.model';
import { Square } from '../../models/square.model';
import { Move } from '../../models/move.model';
import { Board } from '../../models/board.model';

@Component({
  selector: 'app-grid',
  templateUrl: './grid.component.html',
  styleUrls: ['./grid.component.less']
})
export class GridComponent implements OnInit {
  @Input() board: Square[][];
  @Input() rack: string;
  @Input() move: Move;
  @Input() letters: Record<string, number>;
  @Output() publish: EventEmitter<Board> = new EventEmitter<Board>();

  selectedCell: Square;
  hasFocus: boolean;
  direction: number;

  static readonly NONE: number = 0;
  static readonly HORIZONTAL: number = 1;
  static readonly VERTICAL: number = 2;


  @ViewChild('container', { static: false })
  private container: ElementRef<HTMLDivElement>;

  constructor() {
    this.selectedCell = null;
    this.hasFocus = false;
    if (!this.rack) this.rack = "";
  }

  ngOnInit() {
  }

  ngOnChanges() {

  }

  isSelected(tile: Square) {
    if (!this.selectedCell) return false;
    return (tile.position.x == this.selectedCell.position.x && tile.position.y == this.selectedCell.position.y);
  }

  selectCell(tile: Square) {
    if (this.isSelected(tile)) {
      switch (this.direction) {
        case GridComponent.NONE || GridComponent.HORIZONTAL:
          this.direction++;
          break;
        default:
          this.direction = GridComponent.NONE;
          this.selectedCell = null;
          break;
      }
    } else {
      this.direction = GridComponent.HORIZONTAL;
      this.selectedCell = tile;
    }
  }

  gridKeyHandler(event: KeyboardEvent) {
    if (!this.selectedCell || !this.hasFocus)
      return;

    if (event.keyCode >= 'A'.charCodeAt(0) && event.keyCode <= 'z'.charCodeAt(0)) {
      this.selectedCell.tile = event.key.toUpperCase();
      this.selectedCell.value = this.letters[event.key.toUpperCase()];
      this.selectedCell = this.getNextTile(false);
    }

    if (event.keyCode == 8 || event.keyCode == 46) {
      this.selectedCell.tile = "";
      this.selectedCell.value = 0;
      this.selectedCell.isPlayed = false;
      this.selectedCell = this.getNextTile(true);
    }
  }

  rackKeyHandler(event: KeyboardEvent) {
    //if (event.keyCode >= 'A'.charCodeAt(0) && event.keyCode <= 'z'.charCodeAt(0)) 
    this.rack = (event.target as HTMLInputElement).value.toUpperCase();
  }

  private getNextTile(backspace: boolean): Square {
    if (this.direction == GridComponent.HORIZONTAL &&
      ((this.selectedCell.position.x == 14 && !backspace) || (this.selectedCell.position.x == 0 && backspace)))
      return this.selectedCell;
    if (this.direction == GridComponent.VERTICAL &&
      ((this.selectedCell.position.y == 14 && !backspace) || (this.selectedCell.position.y == 0 && backspace)))
      return this.selectedCell;

    let offset = backspace ? -1 : 1;
    let x: number, y: number;
    switch (this.direction) {
      case GridComponent.HORIZONTAL:
        x = this.selectedCell.position.x + offset;
        y = this.selectedCell.position.y;
        break;
      case GridComponent.VERTICAL:
        x = this.selectedCell.position.x;
        y = this.selectedCell.position.y + offset;
    }

    for (var r = 0; r < 15; r++)
      for (let c = 0; c < 15; c++)
        if (this.board[r][c].position.x == x && this.board[r][c].position.y == y)
          return this.board[r][c];

    return this.selectedCell;
  }

  private getPlayedTiles(): Board {
    let tiles: Square[] = [];
    for (var r = 0; r < 15; r++)
      for (let c = 0; c < 15; c++)
        if (this.board[r][c].tile.length)
          tiles.push(this.board[r][c]);

    return { tiles: tiles, rack: this.rack, playedWord: null };
  }




}
