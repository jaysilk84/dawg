import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import * as d3 from 'd3';


@Component({
  selector: 'app-dawg-chart',
  templateUrl: './dawg-chart.component.html',
  styleUrls: ['./dawg-chart.component.less']
})
export class DawgChartComponent implements OnInit, AfterViewInit {

  @ViewChild('chart', { static: false })
  chart: ElementRef;

  constructor() { }

  ngOnInit() {
  }

  ngAfterViewInit() {
    this.buildChartNew();
  }


  buildChartNew() {
    let links = [{ source: "Microsoft", target: "Amazon", type: "licensing", linknum: 0 },
    { source: "Microsoft", target: "Amazon", type: "suit", linknum: 0 },
    { source: "Samsung", target: "Apple", type: "suit", linknum: 0 },
    { source: "Microsoft", target: "Amazon", type: "resolved", linknum: 0 }];

    //sort links by source, then target
    links.sort(function (a, b) {
      if (a.source > b.source) { return 1; }
      else if (a.source < b.source) { return -1; }
      else {
        if (a.target > b.target) { return 1; }
        if (a.target < b.target) { return -1; }
        else { return 0; }
      }
    });

    //any links with duplicate source and target get an incremented 'linknum'
    for (var i = 0; i < links.length; i++) {
      if (i != 0 &&
        links[i].source == links[i - 1].source &&
        links[i].target == links[i - 1].target) {
        links[i].linknum = links[i - 1].linknum + 1;
      }
      else { links[i].linknum = 1; };
    };

    let nodes = {} as any;
    // Compute the distinct nodes from the links.
    links.forEach(function (link) {
      link.source = nodes[link.source] || (nodes[link.source] = { name: link.source });
      link.target = nodes[link.target] || (nodes[link.target] = { name: link.target });
    });

    const margin = {
      top: 40,
      bottom: 10,
      left: 20,
      right: 20
    };

    const width = 800 - margin.left - margin.right;
    const height = 600 - margin.top - margin.bottom;

    // Creates sources <svg> element and inner g (for margins)
    const svg = d3.select(this.chart.nativeElement).append('svg')
      .attr('width', width + margin.left + margin.right)
      .attr('height', height + margin.top + margin.bottom)
      .append('g')
      .attr('transform', `translate(${margin.left}, ${margin.top})`);

    /////////////////////////

    // let force = d3.forceSimulation()
    // .nodes(d3.values(nodes)) as any;

    // force
    // .links(links)
    // //.size([w, h])
    // .linkDistance(60)
    // .charge(-300);
    // //.on("tick", tick)
    // //.start();

    const simulation = d3.forceSimulation()
      .force('link', d3.forceLink().distance(60).id((d: any) => d.name))
      .force('charge', d3.forceManyBody())
      .force('center', d3.forceCenter(width / 2, height / 2)) as any;

    // Links data join
    const link = svg.selectAll('.link').data(links).join(
      (enter) => enter.append('svg:path')
        .attr('class', 'link')
        .attr("class", function (d) { return "link " + d.type; })
    );

    // Nodes data join
    const node = svg.selectAll('.node').data(d3.values(nodes)).join(
      (enter) => {
        const node_enter = enter.append('g');

        node_enter.append('circle')
          .attr('class', 'node')
          .attr('r', 10)
          .append('title').text((d: any) => d.name);
        node_enter.append('text')
          .attr('class', 'title')
          .attr("dy", ".35em")
          .attr("text-anchor", "middle")
          .text(function (d: any) { return d.name.substring(0, 2); });
        return node_enter;
      });

    const edgepaths = svg.selectAll(".edgepath").data(links).join(
      (enter) => {
        const node_enter = enter.append('path')
          .attr('class', 'edgepath')
          .attr('fill-opacity', 0)
          .attr('stroke-opacity', 0)
          .attr('id', function (d, i) { return 'edgepath' + i });

        return node_enter;
      });

    const edgelabels = svg.selectAll(".edgelabel").data(links).join(
      (enter) => {
        const node_enter = enter.append('text')
          .style("pointer-events", "none")
          .attr('class', 'edgelabel')
          .attr('font-size', 10)
          .attr('fill', '#aaa')
          .attr("dy", "-.35em")
          .attr('id', function (d, i) { return 'edgelabel' + i })
         
          .append('textPath')
          .attr('xlink:href', function (d: any, i: any) { return '#edgepath' + i })
          .style("text-anchor", "middle")
          .style("pointer-events", "none")
          .attr("startOffset", "50%")
          .text(function (d: any) { return d.type });

        return node_enter;
      });

    simulation
      .nodes(d3.values(nodes))
      .force('link').links(links);

    simulation.on('tick', () => {
      link.attr("d", function (d: any) {
        var dx = d.target.x - d.source.x,
          dy = d.target.y - d.source.y,
          dr = (75 / d.linknum);  //linknum is defined above
        return "M" + d.source.x + "," + d.source.y + "A" + dr + "," + dr + " 0 0,1 " + d.target.x + "," + d.target.y;
      });

      node.attr("transform", function (d: any) {
        return "translate(" + d.x + "," + d.y + ")";
      });


      edgepaths.attr('d', function (d: any) {
        //return 'M ' + d.source.x + ' ' + d.source.y + ' L ' + d.target.x + ' ' + d.target.y;
        var dx = d.target.x - d.source.x,
        dy = d.target.y - d.source.y,
        dr = (75 / d.linknum);  //linknum is defined above
        return "M" + d.source.x + "," + d.source.y + "A" + dr + "," + dr + " 0 0,1 " + d.target.x + "," + d.target.y;
      });

      edgelabels.attr('transform', function (d:any) {
        const that = this as any;

        if (d.target.x < d.source.x) {
          var bbox = that.getBBox();

          const rx = bbox.x + bbox.width / 2;
          const ry = bbox.y + bbox.height / 2;
          return 'rotate(180 ' + rx + ' ' + ry + ')';
        }
        else {
          return 'rotate(0)';
        }
      });
    });

  }

}
