import {
  Directive,
  ViewContainerRef,
  OnChanges,
  TemplateRef,
  Input
} from '@angular/core';

@Directive({
  selector: '[appNgMatrix]'
})
export class NgMatrixDirective implements OnChanges {
  @Input() appNgMatrixOf: Array<any[][]>;
  @Input() appNgMatrixRows: number;
  @Input() appNgMatrixCols: number;

  constructor(private container: ViewContainerRef,
    private template: TemplateRef<any>,
  ) { }

  ngOnChanges() {
    if (!this.appNgMatrixOf.length) return;
    this.container.clear();
    for (var r = 0; r < this.appNgMatrixRows; r++)
      for (var c = 0; c < this.appNgMatrixCols; c++)
        this.container.createEmbeddedView(this.template, {
          $implicit: this.appNgMatrixOf[r][c],
          position: { row: r, col: c}
        });



    // for (const input of this.appNgLoopOf) {
    //   this.container.createEmbeddedView(this.template,  {
    //     $implicit: input,
    //     index: this.appNgLoopOf.indexOf(input),
    //    });
    // }
  }



}
