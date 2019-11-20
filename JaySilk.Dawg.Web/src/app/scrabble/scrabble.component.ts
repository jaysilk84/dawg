import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { ScrabbleService } from '../services/scrabble.service';
import { Move } from '../models/move.model';
import { Square } from '../models/square.model';
import { Bonus } from '../models/rules.model';
import { Position } from '../models/position.model';

export interface PlayableBoard {
  board: Square[][];
  move: Move;
  rack: string;   
}

@Component({
  selector: 'app-scrabble',
  templateUrl: './scrabble.component.html',
  styleUrls: ['./scrabble.component.less']
})
export class ScrabbleComponent implements OnInit, AfterViewInit {
  private static readonly CELL_SIZE: number = 25;
  private gridCounter = 1;
  private letters: Record<string, number>;
  private bonuses: Bonus[];
  board: Square[][];
  playableBoards: Array<PlayableBoard>;

  @ViewChild('container', { static: false })
  container: ElementRef<HTMLDivElement>;

  constructor(private scrabbleService: ScrabbleService) {
    this.board = [];
    this.playableBoards = new Array<PlayableBoard>();
  }

  onCellClick(event: any) {

  }

  ngOnInit() {


  }

  ngAfterViewInit() {
    //this.createGrid(this.container, 15, "g1");
    this.scrabbleService.getRules().subscribe(rules => {
      this.letters = rules.letters;
      this.bonuses = rules.bonuses;

      this.scrabbleService.getBoards().subscribe(boards => {
        boards.forEach(playableBoard => {
          var b: Square[][] = [];
          for (let r = 0; r < 15; r++) {
            b[r] = [];
            for (let c = 0; c < 15; c++) {
              let tile: Square = playableBoard.tiles.find(s => s.position.x == c && s.position.y == r) || this.blankTile({x: c, y: r});
              tile.color = this.getBackgroundColor(tile);
              b[r][c] = tile;
            }
          }
          this.playableBoards.push({ board: b, rack: playableBoard.rack, move: playableBoard.playedWord });
        });

        // var b: Square[][] = [];
        // for (let r = 0; r < 15; r++) {
        //   b[r] = [];
        //   for (let c = 0; c < 15; c++) {
        //     let tile: Square = board.find(s => s.position.x == c && s.position.y == r) || { position: { x: c, y: r }, isAnchor: false, tile: "" };
        //     tile.color = this.getBackgroundColor(tile);
        //     tile.value = (this.letters[tile.tile] || "").toString();
        //     b[r][c] = tile;
        //   }
        // }

        //this.board = b;

        // this.initBoard(board, "g1");
        // this.scrabbleService.getMoves().subscribe(moves => {
        //   moves.forEach(m => {
        //     this.gridCounter += 1;
        //     var id = "g" + this.gridCounter;
        //     this.createGrid(this.container, 15, id);
        //     this.applyBonuses(id);
        //     this.initBoard(board, id);
        //     this.processMove(m, id);
        //   });
        // });
      });
    });
  }

  private blankTile(p: Position) : Square {
    return {
      position: { x: p.x, y: p.y},
      isAnchor: false,
      isBlank: false,
      tile: "",
      isPlayed: false,
      value: 0
    };
  }

  private initBoard(board: Square[], gridId: string) {
    board.forEach(s => {
      if (s.isAnchor)
        this.setOutlineColor(gridId, s.position.y, s.position.x, "blue");
      this.setText(gridId, s.position.y, s.position.x, s.tile);
      this.setValue(gridId, s.position.y, s.position.x, this.letters[s.tile]);
    });
  }

  private isBlank(move: Move, position: number) {
    return move.blanks.filter(b => b == position).length;
  }

  private processMove(move: Move, gridId: string) {
    if (move.start.y == move.end.y) {
      const r = move.start.y;
      let c = move.start.x;
      for (let i = 0; i < move.word.length; i++) {
        this.setText(gridId, r, c, move.word[i], "red");
        if (!this.isBlank(move, i))
          this.setValue(gridId, r, c, this.letters[move.word[i]]);
        c++;
      }
    } else {
      let r = move.start.y;
      const c = move.start.x;
      for (let i = 0; i < move.word.length; i++) {
        this.setText(gridId, r, c, move.word[i], "red");
        if (!this.isBlank(move, i))
          this.setValue(gridId, r, c, this.letters[move.word[i]]);
        r++;
      }
    }
    this.setLabel(gridId, "Word: " + move.word + " Score: " + move.score);
  }

  private createGrid(container: ElementRef<HTMLDivElement>, gridSize: number, id: string) {
    const parent = document.createElement("div");
    const label = document.createElement("div");
    parent.id = id;
    parent.classList.add("grid");
    parent.style.height = parent.style.width = ScrabbleComponent.CELL_SIZE * gridSize + "px";
    label.classList.add("label");

    for (let r = 0; r < gridSize; r++)
      for (let c = 0; c < gridSize; c++) {
        const cell = document.createElement("div");
        const content = document.createElement("div");
        const value = document.createElement("div");

        cell.style.height = cell.style.width = ScrabbleComponent.CELL_SIZE - 2 + "px";
        cell.classList.add("row" + r);
        cell.classList.add("col" + c);
        cell.classList.add("cell");
        parent.appendChild(cell);

        content.classList.add("content");
        content.style.height = "100%";
        content.style.textAlign = "right";
        content.style.width = (ScrabbleComponent.CELL_SIZE / 2) + 3 + "px";
        content.style.lineHeight = ScrabbleComponent.CELL_SIZE + "px";
        cell.appendChild(content);

        value.style.cssFloat = "right";
        value.style.fontSize = "8px";
        value.style.lineHeight = "10px";
        value.classList.add("val");
        cell.appendChild(value);

      }
    parent.appendChild(label);
    container.nativeElement.appendChild(parent);
  }
  private applyBonuses(id: string) {
    this.bonuses.forEach(b => {
      switch (b.type) {
        case 0:
          b.value == 2 ? this.setBackground(id, b.position.y, b.position.x, 52, 113, 235) :
            this.setBackground(id, b.position.y, b.position.x, 52, 235, 73);
          break;
        case 1:
          b.value == 2 ? this.setBackground(id, b.position.y, b.position.x, 235, 52, 52) :
            this.setBackground(id, b.position.y, b.position.x, 235, 211, 52);
      }
    });
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
    const element = <HTMLDivElement>grid.getElementsByClassName(classSelector)[0].getElementsByClassName("content")[0];
    element.innerText = text;
    //element.style.lineHeight = element.clientHeight + "px";
    element.style.color = color;
  }
  private setLabel(id: string, text: string) {
    document.querySelector("#" + id + " .label").innerHTML = text;
  }
  private setValue(id: string, row: number, col: number, value: number) {
    const classSelector = "row" + row + " col" + col;
    const grid = document.getElementById(id);
    const text = value ? value.toString() : '';
    (<HTMLDivElement>grid.getElementsByClassName(classSelector)[0].getElementsByClassName("val")[0]).innerText = text;
  }

  private getBackgroundColor(s: Square) {
    //let bonus = this.bonuses.find(b => b.position.x == s.position.x && b.position.y == s.position.y) || { position: null, value: null, type: -1 };
    let bonus = this.bonuses[s.position.x + ", " + s.position.y] || { position: null, value: null, type: -1 };

    switch (bonus.type) {
      case 0:
        return bonus.value == 2 ? "rgba(52, 113, 235, .2)" : "rgba(52, 235, 73, .2)";
      case 1:
        return bonus.value == 2 ? "rgba(235, 52, 52, .2)" : "rgba(235, 211, 52, .2)";
      default:
        return "rgba(255, 255, 255)";
    }
  }
}
