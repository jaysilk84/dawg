import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as d3 from 'd3';
import { stringify } from '@angular/compiler/src/util';
import { min } from 'd3';


const localUrl = 'http://localhost:5000/graph';

@Component({
  selector: 'app-dawg-chart',
  templateUrl: './dawg-chart.component.html',
  styleUrls: ['./dawg-chart.component.less'],
  //providers: [HttpClient]
})
export class DawgChartComponent implements OnInit, AfterViewInit {

  @ViewChild('chart', { static: false })
  chart: ElementRef;

  constructor(private http: HttpClient) { }

  ngOnInit() {
  }

  ngAfterViewInit() {
    this.http.get(localUrl).subscribe((data) => this.buildChartNew(data));

    //this.buildChartNew();
  }


  buildChartNew(links: any) {
    // let links = [{ source: "Microsoft", target: "Amazon", type: "licensing", linknum: 0 },
    // { source: "Microsoft", target: "Amazon", type: "suit", linknum: 0 },
    // { source: "Samsung", target: "Apple", type: "suit", linknum: 0 },
    // { source: "Microsoft", target: "Amazon", type: "resolved", linknum: 0 },
    // { source: "Microsoft", target: "Apple", type: "suit", linknum: 0 },
    // ] as any;

    let noTargetLinks = links.filter(d => d.target == null);
    links = links.filter(d => d.target);

    //sort links by source, then target
    links.sort(function (a, b) {
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

    let nodes = {} as any;
    // Compute the distinct nodes from the links.
    links.forEach(function (link) {
      link.source = nodes[link.source.id] || (nodes[link.source.id] = { isRoot: link.source.isRoot, id: link.source.id, name: link.source.id.toString(), endOfWord: link.source.endOfWord });
      link.target = nodes[link.target.value.id] || (nodes[link.target.value.id] = { isRoot: link.target.value.isRoot, id: link.target.value.id, name: link.target.value.id.toString(), endOfWord: link.target.value.endOfWord });
    });

    noTargetLinks.forEach(link => nodes[link.source.id] = { name: link.source.id.toString(), endOfWord: link.source.endOfWord });
    //links.forEach((link) => nodes[link.id] = {name: link.id, endOfWord: link.endOfWord});

    let tnodes = d3.values(nodes);
    var nodeById = d3.map(tnodes, function (d: any) { return d.name; }),
      bilinks = [];

    links.forEach(function (link) {
      var key = link.key,
        linknum = link.linknum,
        s = link.source = nodeById.get(link.source.name),
        t = link.target = nodeById.get(link.target.name),
        i = {}; // intermediate node
      tnodes.push(i);
      links.push({ source: s, target: i }, { source: i, target: t });
      bilinks.push([s, i, t, key, linknum]);
    });

    const margin = {
      top: 40,
      bottom: 10,
      left: 20,
      right: 20
    };

    const width = 1920 - margin.left - margin.right;
    const height = 1080 - margin.top - margin.bottom;

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
        .attr("id", (d: any) => d)
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
        .attr("class", function (d) { return "link " + d[3]; })
        .attr("marker-end", "url(#end)")
    );

    const edgepaths = svg.selectAll(".edgepath").data(bilinks).join(
      (enter) => {
        const node_enter = enter.append('path')
          .attr('class', 'edgepath')
          .attr('fill-opacity', 0)
          .attr('stroke-opacity', 0)
          .attr('id', function (d, i) { return 'edgepath' + i });

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
          .attr('id', function (d, i) { return 'edgelabel' + i })

          .append('textPath')
          .attr('xlink:href', function (d: any, i: any) { return '#edgepath' + i })
          .style("text-anchor", "middle")
          .style("pointer-events", "none")
          .attr("startOffset", "50%")
          .text(function (d: any) { return d[3] });

        return node_enter;
      });

    // Nodes data join
    const node = svg.selectAll('.node').data(tnodes.filter((d: any) => d.name)).join(
      (enter) => {
        const node_enter = enter.append('g')
          .call(d3.drag()
            .on("start", dragstarted)
            .on("drag", dragged)
            .on("end", dragended));

        node_enter.append('circle')
          .attr('class', (d: any) => {
            if (d.isRoot) return 'node root';
            return 'node ' + (d.endOfWord ? 'endOfWord' : '');
          })
          .attr('r', 10)
          .append('title').text((d: any) => d.name);

        node_enter.append('text')
          .attr('class', 'title')
          .attr("dy", ".35em")
          .attr("text-anchor", "middle")
          .text(function (d: any) { return d.name; });
        return node_enter;
      });

    const simulation = d3.forceSimulation()
      .nodes(tnodes)
      .force('link', d3.forceLink(links).distance(80).id((d: any) => d.name))
      .force('collide', d3.forceCollide().radius(2).strength(5))
      .force('charge', d3.forceManyBody().strength(-140).distanceMax(150).distanceMin(30))
      .force('center', d3.forceCenter(width / 2, height / 2)) as any;

    //simulation
    //.nodes(d3.values(nodes))
    //.force('link').links(links);

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
      // link.attr("d", function (d: any) {
      //   var dx = d.target.x - d.source.x,
      //     dy = d.target.y - d.source.y,
      //     dr = (100 / d.linknum);  //linknum is defined above
      //   return "M" + d.source.x + "," + d.source.y + "A" + dr + "," + dr + " 0 0,1 " + d.target.x + "," + d.target.y;
      // });

      var renderLinks = function (d: any) {
        return "M" + d[0].x + "," + d[0].y
          + "S" + d[1].x + "," + d[1].y
          + " " + d[2].x + "," + d[2].y;
      };

      var renderLinksBetter = function (d: any) {
        const weight = 2;
        var source = d[0];
        var target = d[2];
        var linknum = d[4];

        var dx = target.x - source.x,
          dy = target.y - source.y;
        var qx = dy / weight * linknum, //linknum is defined above
          qy = -dx / weight * linknum;
        var qx1 = (source.x + (dx / 2)) + qx,
          qy1 = (source.y + (dy / 2)) + qy;

        return "M" + source.x + " " + source.y +
          " C" + source.x + " " + source.y + " " +
          qx1 + " " + qy1 + " " +
          target.x + " " + target.y;
      }

      link.attr("d", renderLinksBetter);

      node.attr("transform", (d: any) => {
        return "translate(" + d.x + "," + d.y + ")";
      });
      // node
      //   .attr("cx", (d:any) => d.x)
      //   .attr("cy", (d:any) => d.y);


      edgepaths.attr('d', renderLinksBetter);

      // function (d: any) {
      //   //return 'M ' + d.source.x + ' ' + d.source.y + ' L ' + d.target.x + ' ' + d.target.y;
      //   var dx = d.target.x - d.source.x,
      //     dy = d.target.y - d.source.y,
      //     dr = (100 / d.linknum);  //linknum is defined above
      //   return "M" + d.source.x + "," + d.source.y + "A" + dr + "," + dr + " 0 0,1 " + d.target.x + "," + d.target.y;
      // });

      edgelabels.attr('transform', function (d: any) {
        const that = this as any;

        if (d[2].x < d[0].x) {
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
