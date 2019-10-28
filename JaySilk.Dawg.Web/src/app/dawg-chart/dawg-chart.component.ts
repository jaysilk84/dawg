import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { GraphService } from '../services/graph.service';

import * as d3 from 'd3';
import { Edge } from '../models/edge.model';
import { Vertex } from '../models/vertex.model';
import { BiLink } from '../models/biLink.model';
import { switchMap, delay } from "rxjs/operators"

@Component({
  selector: 'app-dawg-chart',
  templateUrl: './dawg-chart.component.html',
  styleUrls: ['./dawg-chart.component.less'],
})
export class DawgChartComponent implements OnInit, AfterViewInit {

  @ViewChild('chart', { static: false })
  chart: ElementRef;

  constructor(private graph: GraphService, private route: ActivatedRoute, private router: Router) { }

  ngOnInit() {
  }

  ngAfterViewInit() {
    // this.router.events.subscribe(e => {
    //   if (e instanceof NavigationEnd) {
    //     this.route.queryParamMap.pipe(
    //       switchMap((params) => {
    //         const numwords: number = +params.get("numwords") || 10;
    //         const batchsize: number = +params.get("batchsize") || 2;
    //         return this.graph.getEdges(numwords, batchsize);
    //       })

    //     ).subscribe((data) => this.buildChart(data));
    //   }
    // });
    
    this.graph.messageReceived.subscribe((data: Edge[]) => this.buildChart(data));

  }


  buildChart(links: Array<Edge>) {

    const noTargetLinks = links.filter((d) => d.target == null);
    links = links.filter((d) => d.target);

    //sort links by source, then target
    links.sort((a, b) => {
      if (a.source.id > b.source.id) { return 1; }
      else if (a.source.id < b.source.id) { return -1; }
      else {
        if (a.target.id > b.target.id) { return 1; }
        if (a.target.id < b.target.id) { return -1; }
        else { return 0; }
      }
    });

    //any links with duplicate source and target get an incremented 'linknum'
    for (var i = 0; i < links.length; i++) {
      if (i != 0 &&
        links[i].source.id == links[i - 1].source.id &&
        links[i].target.id == links[i - 1].target.id) {
        links[i].linknum = links[i - 1].linknum + 1;
      }
      else { links[i].linknum = 1; };
    };

    const nodes = new Set<Vertex>();

    // Compute the distinct nodes from the links.
    links.forEach((link) => {
      nodes[link.source.id] = link.source;
      nodes[link.target.id] = link.target;
    });

    noTargetLinks.forEach(link => nodes[link.source.id] = link.source);

    const tnodes: Array<Vertex> = d3.values(nodes);
    const nodeById = d3.map(tnodes, function (d) { return d.id.toString(); });
    const bilinks = new Array<BiLink>();

    links.forEach(function (link) {
      const key: string = link.key,
        linknum: number = link.linknum,
        s: Vertex = link.source = nodeById.get(link.source.id.toString()),
        t: Vertex = link.target = nodeById.get(link.target.id.toString()),
        i: Vertex = { id: null, endOfWord: null, isRoot: null }; // intermediate node
      tnodes.push(i);
      links.push({ source: s, target: i, key: key, linknum: 0 }, { source: i, target: t, key: key, linknum: 0 });
      bilinks.push({ source: s, intermediate: i, target: t, key: key, linknum: linknum });
    });

    const margin = {
      top: 40,
      bottom: 10,
      left: 20,
      right: 20
    };

    const width = 1920 - margin.left - margin.right;
    const height = 1080 - margin.top - margin.bottom;

    d3.selectAll("svg").remove();

    // Creates sources <svg> element and inner g (for margins)
    const svg = d3.select(this.chart.nativeElement).append('svg')
      .attr('width', width + margin.left + margin.right)
      .attr('height', height + margin.top + margin.bottom)
      .append('g')
      .attr('transform', `translate(${margin.left}, ${margin.top})`);


    /////////////////////////
    const defs = svg.append("svg:defs").selectAll("marker")
      .data(["end"])
      .join((enter) => enter.append("svg:marker")
        .attr("id", (d) => d)
        .attr("viewBox", "0 -5 10 10")
        .attr("refX", 20)
        .attr("refY", -2)
        .attr("markerWidth", 6)
        .attr("markerHeight", 6)
        .attr("orient", "auto")
        .append("svg:path")
        .attr("d", "M0,-5L10,0L0,5")
      );

    // Links data join
    const link = svg.selectAll('.link').data(bilinks).join(
      (enter) => enter.append('svg:path')
        .attr("class", (d) => "link " + d.key)
        .attr("marker-end", "url(#end)")
    );

    const edgepaths = svg.selectAll(".edgepath").data(bilinks).join(
      (enter) => {
        const node_enter = enter.append('path')
          .attr('class', 'edgepath')
          .attr('fill-opacity', 0)
          .attr('stroke-opacity', 0)
          .attr('id', (d, i) => 'edgepath' + i.toString());

        return node_enter;
      });

    const edgelabels = svg.selectAll(".edgelabel").data(bilinks).join(
      (enter) => {
        const node_enter = enter.append('text')
          .style("pointer-events", "none")
          .attr('class', 'edgelabel')
          .attr('font-size', "50")
          .attr('fill', '#000')
          .attr("dy", "-.35em")
          .attr('id', (d, i) => 'edgelabel' + i.toString())

          .append('textPath')
          .attr('xlink:href', (d, i) => '#edgepath' + i.toString())
          .style("text-anchor", "middle")
          .style("pointer-events", "none")
          .attr("startOffset", "50%")
          .text((d) => d.key );

        return node_enter;
      });

    // Nodes data join
    const node = svg.selectAll('.node').data(tnodes.filter((d) => d.id)).join(
      (enter) => {
        const node_enter = enter.append('g')
          .call(d3.drag()
            .on("start", dragstarted)
            .on("drag", dragged)
            .on("end", dragended));

        node_enter.append('circle')
          .attr('class', (d) => {
            if (d.isRoot) return 'node root';
            return 'node ' + (d.endOfWord ? 'endOfWord' : '');
          })
          .attr('r', 10)
          .append('title').text((d) => d.id.toString());

        node_enter.append('text')
          .attr('class', 'title')
          .attr("dy", ".35em")
          .attr("text-anchor", "middle")
          .text((d) =>  d.id.toString());
        return node_enter;
      });

    const simulation = d3.forceSimulation()
      .nodes(tnodes)
      .force('link', d3.forceLink(links).distance(80))//.id((d: Vertex) => d.id ? d.id.toString() : null))
      .force('collide', d3.forceCollide().radius(2).strength(5))
      .force('charge', d3.forceManyBody().strength(-140).distanceMax(150).distanceMin(30))
      .force('center', d3.forceCenter(width / 2, height / 2));

    // Drag stuff
    function dragstarted(d) {
      if (!d3.event.active) simulation.alphaTarget(0.3).restart();
      d.fx = d.x;
      d.fy = d.y;
    }

    function dragged(d) {
      d.fx = d3.event.x;
      d.fy = d3.event.y;
    }

    function dragended(d) {
      if (!d3.event.active) simulation.alphaTarget(0);

      d.fx = null;
      d.fy = null;
    }


    simulation.on('tick', () => {

      const renderLinksBetter = (d: BiLink) => {
        const weight = 2;
        const source = d.source;
        const target = d.target;
        const linknum = d.linknum;

        const dx = target.x - source.x,
          dy = target.y - source.y;
        const qx = dy / weight * linknum, //linknum is defined above
          qy = -dx / weight * linknum;
        const qx1 = (source.x + (dx / 2)) + qx,
          qy1 = (source.y + (dy / 2)) + qy;

        return "M" + source.x + " " + source.y +
          " C" + source.x + " " + source.y + " " +
          qx1 + " " + qy1 + " " +
          target.x + " " + target.y;
      }

      link.attr("d", renderLinksBetter);

      node.attr("transform", (d) => "translate(" + d.x + "," + d.y + ")");

      edgepaths.attr('d', renderLinksBetter);

      edgelabels.attr('transform', function (this: SVGGraphicsElement, d) {
        if (d.target.x < d.source.x) {
          const bbox = this.getBBox();

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
