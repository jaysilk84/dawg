import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { ScrabbleService } from '../services/scrabble.service';
import { Move } from '../models/move.model';
import { Square } from '../models/square.model';

@Component({
  selector: 'app-scrabble',
  templateUrl: './scrabble.component.html',
  styleUrls: ['./scrabble.component.less']
})
export class ScrabbleComponent implements OnInit, AfterViewInit {
  private static readonly CELL_SIZE: number = 25;
  private gridCounter = 1;

  @ViewChild('container', { static: false })
  container: ElementRef<HTMLDivElement>;

  constructor(private scrabbleService: ScrabbleService) {
  }

  ngOnInit() {
  }

  ngAfterViewInit() {
    this.createGrid(this.container, 15, "1");
    this.scrabbleService.getBoard().subscribe(board => {
      this.initBoard(board, "1");
      this.scrabbleService.getMoves().subscribe(moves => {
        moves.forEach(m => {
          this.gridCounter+=1;
          this.createGrid(this.container, 15, this.gridCounter.toString());
          this.initBoard(board, this.gridCounter.toString());
          this.processMove(m, this.gridCounter.toString());
        });
      });
    });
  }

  private initBoard(board: Square[], gridId: string) {
    board.forEach(s => {
      if (s.isAnchor)
        this.setOutlineColor(gridId, s.position.y, s.position.x, "blue");
      this.setText(gridId, s.position.y, s.position.x, s.tile);
    });
  }

  private processMove(move: Move, gridId: string) {
    if (move.start.y == move.end.y) {
      const r = move.start.y;
      let c = move.start.x;
      for (let i = 0; i < move.word.length; i++) {
        this.setText(gridId, r, c, move.word[i], "red");
        c++;
      }
    } else {
      let r = move.start.y;
      const c = move.start.x;
      for (let i = 0; i < move.word.length; i++) {
        this.setText(gridId, r, c, move.word[i], "red");
        r++;
      }
    }
  }

  private createGrid(container: ElementRef<HTMLDivElement>, gridSize: number, id: string) {
    const parent = document.createElement("div");
    parent.id = id;
    parent.classList.add("grid");
    parent.style.height = parent.style.width = ScrabbleComponent.CELL_SIZE * gridSize + "px";

    for (let r = 0; r < gridSize; r++)
      for (let c = 0; c < gridSize; c++) {
        const cell = document.createElement("div");
        cell.style.height = cell.style.width = ScrabbleComponent.CELL_SIZE - 2 + "px";
        cell.classList.add("row" + r);
        cell.classList.add("col" + c);
        cell.classList.add("cell");
        parent.appendChild(cell);
      }

    container.nativeElement.appendChild(parent);
  }

  private setOutlineColor(id: string, row: number, col: number, color: string) {
    const classSelector = "row" + row + " col" + col;
    const grid = document.getElementById(id);
    (<HTMLDivElement>grid.getElementsByClassName(classSelector)[0]).style.borderColor = color;
  }
  private setBackground(id: string, row: number, col: number, r: number, g: number, b: number) {
    const classSelector = "row" + row + " col" + col;
    const grid = document.getElementById(id);
    (<HTMLDivElement>grid.getElementsByClassName(classSelector)[0]).style.backgroundColor = "rgba(" + r + "," + g + "," + b + ",.2)";
  }
  private setText(id: string, row: number, col: number, text: string, color: string = "#000") {
    const classSelector = "row" + row + " col" + col;
    const grid = document.getElementById(id);
    const element = <HTMLDivElement>grid.getElementsByClassName(classSelector)[0];
    element.innerText = text;
    element.style.lineHeight = element.clientHeight + "px";
    element.style.color = color;
  }
}
