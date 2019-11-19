import { Component, OnInit, OnChanges, Input, Renderer2, ViewChild, ElementRef} from '@angular/core';
import { Bonus } from '../../models/rules.model'
@Component({
  selector: 'app-grid',
  templateUrl: './grid.component.html',
  styleUrls: ['./grid.component.less']
})
export class GridComponent implements OnInit {
  someMatrix = [["1", "2", "3"], ["4", "5", "6"], ["7", "8", "9"]];
  @Input() board: number[][];
  @Input() bonuses: Bonus[]; 
  @ViewChild('container', { static: false })
  private container: ElementRef<HTMLDivElement>;

  constructor(private renderer: Renderer2) { }

  ngOnInit() {
  }

  ngOnChanges() {

  }


}
