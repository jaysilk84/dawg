import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';

@Component({
  selector: 'app-scrabble',
  templateUrl: './scrabble.component.html',
  styleUrls: ['./scrabble.component.less']
})
export class ScrabbleComponent implements OnInit, AfterViewInit {
  private static readonly CELL_SIZE: number = 25;

  @ViewChild('container', { static: false })
  container: ElementRef<HTMLDivElement>;

  constructor() {
  }

  ngOnInit() {
  }

  ngAfterViewInit() {
    this.createGrid(this.container, 15, "1");
    this.createGrid(this.container, 15, "2");
    this.createGrid(this.container, 15, "3");
    this.createGrid(this.container, 15, "4");
    this.createGrid(this.container, 15, "5");
    this.createGrid(this.container, 15, "6");
    this.createGrid(this.container, 15, "7");

    this.setOutlineColor("1", 1, 1, "red");
    this.setText("1", 1, 1, "J");
    this.setBackground("1", 1, 1, 5, 250, 5);

    this.setOutlineColor("3", 1, 1, "red");
    this.setText("3", 1, 1, "J");
    this.setBackground("7", 1, 1, 5, 250, 5);
  }

  private createGrid(element: ElementRef<HTMLDivElement>, gridSize: number, id: string) {
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

    element.nativeElement.appendChild(parent);
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
  private setText(id: string, row: number, col: number, text: string) {
    const classSelector = "row" + row + " col" + col;
    const grid = document.getElementById(id);
    const element = <HTMLDivElement>grid.getElementsByClassName(classSelector)[0];
    element.innerText = text;
    element.style.lineHeight = element.clientHeight + "px";
  }
}
