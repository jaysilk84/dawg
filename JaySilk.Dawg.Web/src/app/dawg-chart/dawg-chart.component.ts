import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { GraphService } from '../services/graph.service';

import * as d3 from 'd3';
import { Edge } from '../models/edge.model';
import { Vertex } from '../models/vertex.model';
import { BiLink } from '../models/biLink.model';
import { switchMap, delay } from "rxjs/operators"
import { disableDebugTools } from '@angular/platform-browser';

@Component({
  selector: 'app-dawg-chart',
  templateUrl: './dawg-chart.component.html',
  styleUrls: ['./dawg-chart.component.less'],
})
export class DawgChartComponent implements OnInit, AfterViewInit {

  @ViewChild('chart', { static: false })
  chart: ElementRef;
  private static readonly DPI: number = window.devicePixelRatio;

  @ViewChild('canvas', { static: false })
  canvasElement: ElementRef<HTMLCanvasElement>;

  constructor(private graph: GraphService, private route: ActivatedRoute, private router: Router) { }

  ngOnInit() {
  }

  ngAfterViewInit() {
    this.router.events.subscribe(e => {
      if (e instanceof NavigationEnd) {
        this.route.queryParamMap.pipe(
          switchMap((params) => {
            const numwords: number = +params.get("numwords") || 10;
            const batchsize: number = +params.get("batchsize") || 2;
            return this.graph.getEdges(numwords, batchsize);
          })

        )//.subscribe((data) => this.buildChart(data));
          .subscribe((data) => this.buildChartCanvas(data));
      }
    });

    //this.graph.messageReceived.subscribe((data: Edge[]) => this.buildChart(data));

  }

  setCanvasDpi(canvas, dpi = DawgChartComponent.DPI) {
    const style_height = +getComputedStyle(canvas).getPropertyValue("height").slice(0, -2);
    const style_width = +getComputedStyle(canvas).getPropertyValue("width").slice(0, -2);
    //console.log("dpi: " + dpi);
    canvas.height = style_height * dpi;
    canvas.width = style_width * dpi;
    canvas.style.height = style_height + 'px';
    canvas.style.width = style_width + 'px';
    canvas.getContext('2d').setTransform(dpi, 0, 0, dpi, 0, 0);
  }

  buildChartCanvas(links: Edge[]) {
    const data = this.formatData(links);
    const nodes: Vertex[] = data[0];
    const edges: Edge[] = data[1];
    const nodeById = d3.map(nodes, function (d) { return d.id.toString(); });
    const canvas = this.canvasElement.nativeElement;
    const context = canvas.getContext("2d");
    const self = this;

    self.setCanvasDpi(canvas, window.devicePixelRatio);
    context.setTransform(DawgChartComponent.DPI, 0, 0, DawgChartComponent.DPI, 0, 0);

    edges.forEach(e => {
      e.source = nodeById.get(e.source.id.toString());
      e.target = nodeById.get(e.target.id.toString());
    });

    let width = canvas.width,
      height = canvas.height;

    var simulation = d3.forceSimulation()
      .nodes(nodes)
      .force("link", d3.forceLink<Vertex, Edge>(edges).distance(80))
      //.force("link", d3.forceLink<Vertex, Edge>(edges).distance(d => 80))
      .force('collide', d3.forceCollide().radius(15).strength(5).iterations(1))
      .force('charge', d3.forceManyBody().strength(-340).distanceMax(200).distanceMin(80))
      //.force('charge', (d:any) => d.r * d.r * 1000 * -1.3)
      .force('center', d3.forceCenter(width / 2, height / 2))
      .on("tick", ticked);

    d3.select(canvas)
      .call(d3.drag()
        .container(canvas)
        .subject(dragsubject)
        .on("start", dragstarted)
        .on("drag", dragged)
        .on("end", dragended));

    function ticked() {
      context.clearRect(0, 0, width, height);

      context.beginPath();
      edges.forEach(drawLink);

      // regular nodes
      nodes.filter(n => !n.isRoot && !n.endOfWord).forEach((n) => drawNode(n, "#ccc"));
      // root node
      nodes.filter(n => n.isRoot).forEach((n) => drawNode(n, "green"));
      // end of word nodes
      nodes.filter(n => n.endOfWord).forEach((n) => drawNode(n, "red"));
    }

    function dragsubject() {
      return simulation.find(d3.event.x, d3.event.y);
    }

    function dragstarted() {
      if (!d3.event.active) simulation.alphaTarget(0.3).restart();
      d3.event.subject.fx = d3.event.subject.x;
      d3.event.subject.fy = d3.event.subject.y;
    }

    function dragged() {
      d3.event.subject.fx = d3.event.x;
      d3.event.subject.fy = d3.event.y;
    }

    function dragended() {
      if (!d3.event.active) simulation.alphaTarget(0);
      d3.event.subject.fx = null;
      d3.event.subject.fy = null;
    }


    function drawCurveLink(fromx, fromy, tox, toy, linknum, ctx) {
      const weight = 2;
      const dx = tox - fromx,
        dy = toy - fromy;
      const qx = dy / weight * linknum,
        qy = -dx / weight * linknum;
      const qx1 = (fromx + (dx / 2)) + qx,
        qy1 = (fromy + (dy / 2)) + qy;

      ctx.beginPath();
      ctx.moveTo(fromx, fromy);
      ctx.quadraticCurveTo(qx1, qy1, tox, toy);
      ctx.strokeStyle = "#666";
      ctx.lineWidth = 1.5;
      ctx.stroke();

      return { x: qx1, y: qy1 };
    }

    function drawArrow(ctx, fromx, fromy, tox, toy, offset) {
      var angle = Math.atan2(toy - fromy, tox - fromx);
      var headlen = 10;

      tox = tox + offset * Math.cos(angle - Math.PI);
      toy = toy + offset * Math.sin(angle - Math.PI);
      fromx = fromx - offset * Math.cos(angle - Math.PI);
      fromy = fromy - offset * Math.sin(angle - Math.PI);

      //starting a new path from the head of the arrow to one of the sides of the point
      ctx.beginPath();
      ctx.moveTo(tox, toy);
      ctx.lineTo(tox - (headlen * Math.cos(angle - Math.PI / 7)), toy - headlen * Math.sin(angle - Math.PI / 7));

      //path from the side point of the arrow, to the other side point
      ctx.lineTo(tox - (headlen * Math.cos(angle + Math.PI / 7)), toy - headlen * Math.sin(angle + Math.PI / 7));

      //path from the side point back to the tip of the arrow, and then again to the opposite side point
      ctx.lineTo(tox, toy);
      ctx.lineTo(tox - (headlen * Math.cos(angle - Math.PI / 7)), toy - headlen * Math.sin(angle - Math.PI / 7));

      //draws the paths created above
      ctx.strokeStyle = "#666";
      ctx.lineWidth = 1.5;
      ctx.stroke();
      ctx.fillStyle = "#666";
      ctx.fill();
    }

    function drawLine(ctx, fromx, fromy, tox, toy) {
      ctx.beginPath();
      ctx.moveTo(fromx, fromy);
      ctx.lineTo(tox, toy);
      ctx.strokeStyle = "#666";
      ctx.lineWidth = 1.5;
      ctx.stroke();

      //draws the paths created above
      ctx.strokeStyle = "#666";
      ctx.lineWidth = 1.5;
      ctx.stroke();
      ctx.fillStyle = "#666";
      ctx.fill();
    }

    function getCurvePoint(start, control, end, t) {

      var c1x = start.x + (control.x - start.x) * t;
      var c1y = start.y + (control.y - start.y) * t;
      var c2x = control.x + (end.x - control.x) * t;
      var c2y = control.y + (end.y - control.y) * t;
      var tx = c1x + (c2x - c1x) * t;
      var ty = c1y + (c2y - c1y) * t;

      return { x: tx, y: ty };
    }

    function drawText(ctx, text, start, control, end) {
      let textAngle = Math.atan2(end.y - start.y, end.x - start.x);

      const textPos: any = Object.assign({}, ...['x', 'y'].map(c => ({
        [c]: start[c] + (end[c] - start[c]) / 2 // calc middle point
      })));

      if (textAngle > Math.PI / 2) textAngle = -(Math.PI - textAngle);
      if (textAngle < -Math.PI / 2) textAngle = -(-Math.PI - textAngle);

      let tx = 0;
      let ty = 0;
      
      if (control && control.x && control.y) {
        var tp = getCurvePoint(start, control, end, .5);
        tx = tp.x;
        ty = tp.y;
      } else {
        tx = textPos.x;
        ty = textPos.y;
      }
      
      ctx.save();
      ctx.translate(tx, ty);
      ctx.rotate(textAngle);

      ctx.font = "20px sans-serif";
      ctx.fillStyle = "#000";
      //textAlign supports: start, end, left, right, center
      ctx.textAlign = "center"
      //textBaseline supports: top, hanging, middle, alphabetic, ideographic bottom
      ctx.textBaseline = "hanging"
      ctx.fillText(text, 0, 0);
      ctx.restore();
    }

    function drawLink(d) {
      const start = d.source;
      const end = d.target;
      var control = null;
      var ap = null;
     
      //drawLine(context, d.source.x, d.source.y, d.target.x, d.target.y);
      control = drawCurveLink(d.source.x, d.source.y, d.target.x, d.target.y, d.linknum, context);
      
      if (control)
        ap = getCurvePoint(start, control, end, .9);

      var arrowBegin = control ? { x: ap.x, y: ap.y} : {x: start.x, y: start.y};
      drawArrow(context, arrowBegin.x, arrowBegin.y, end.x, end.y, 10);
      //drawArrow(context, start.x, start.y, end.x, end.y, 10);
      drawText(context, d.key, start, control, end);
    }

    function drawNode(d, color) {

      let lineWidth = 1.5;
      context.beginPath();

      context.moveTo(d.x + 10, d.y);
      context.arc(d.x, d.y, 10, 0, 2 * Math.PI);

      context.fillStyle = color;
      context.strokeStyle = "#333";
      context.lineWidth = lineWidth;
      context.fill();
      context.stroke();

      context.beginPath();
      context.font = "10px sans-serif";
      context.textAlign = "center";
      context.textBaseline = "middle";
      context.fillStyle = "#000";
      context.fillText(d.id, d.x, d.y);
    }
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
          .text((d) => d.key);

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
          .text((d) => d.id.toString());
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

  private formatData(links: Edge[]): [Vertex[], Edge[]] {
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

    // const tnodes: Array<Vertex> = d3.values(nodes);
    // const nodeById = d3.map(tnodes, function (d) { return d.id.toString(); });
    // const bilinks = new Array<BiLink>();

    // links.forEach(function (link) {
    //   const key: string = link.key,
    //     linknum: number = link.linknum,
    //     s: Vertex = link.source = nodeById.get(link.source.id.toString()),
    //     t: Vertex = link.target = nodeById.get(link.target.id.toString()),
    //     i: Vertex = { id: null, endOfWord: null, isRoot: null }; // intermediate node
    //   tnodes.push(i);
    //   links.push({ source: s, target: i, key: key, linknum: 0 }, { source: i, target: t, key: key, linknum: 0 });
    //   bilinks.push({ source: s, intermediate: i, target: t, key: key, linknum: linknum });
    // });

    return [d3.values(nodes), links];
  }
}
