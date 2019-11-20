import { Component, OnInit, OnChanges, Input, Renderer2, ViewChild, ElementRef} from '@angular/core';
import { Bonus } from '../../models/rules.model';
import { Square } from '../../models/square.model';
import { Move } from '../../models/move.model';

@Component({
  selector: 'app-grid',
  templateUrl: './grid.component.html',
  styleUrls: ['./grid.component.less']
})
export class GridComponent implements OnInit {
  @Input() board: Square[][];
  @Input() rack: string;
  @Input() move: Move;
  selectedCell: Square;
  hasFocus: boolean;

  @ViewChild('container', { static: false })
  private container: ElementRef<HTMLDivElement>;

  constructor(private renderer: Renderer2) {
    this.selectedCell = null;
    this.hasFocus = false;
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
    this.isSelected(tile) ? this.selectedCell = null : this.selectedCell = tile;
  }

  keyHandler(event) {
    if (!this.selectedCell || !this.hasFocus) 
      return;

    if (event.keyCode >= 'A'.charCodeAt(0) && event.keyCode <= 'z'.charCodeAt(0))
      this.selectedCell.tile = event.key.toUpperCase();
    
    if (event.keyCode == 8 || event.keyCode == 46)
      this.selectedCell.tile = "";
  }






}
