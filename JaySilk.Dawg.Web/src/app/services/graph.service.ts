import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Edge } from '../models/edge.model';

@Injectable({
  providedIn: 'root'
})
export class GraphService {
  readonly graphApiUrl = 'https://localhost:5001/graph';

  constructor(private http: HttpClient) { }

  getEdges(numWords = 10, batchSize = 2): Observable<Edge[]> {
    return this.http.get<Edge[]>(this.graphApiUrl + '?numWords=' + numWords.toString() + '&batchSize=' + batchSize.toString());
  }
}
