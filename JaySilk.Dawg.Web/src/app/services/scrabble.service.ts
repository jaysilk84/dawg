import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Square } from '../models/square.model';
import { Move } from '../models/move.model';
import { Rules } from '../models/rules.model';

@Injectable({
  providedIn: 'root'
})
export class ScrabbleService {
  private readonly baseUrl = 'https://localhost:5001/';
  private readonly boardApiUrl = this.baseUrl + 'scrabble/board';
  private readonly moveApiUrl = this.baseUrl + 'scrabble/move';

  constructor(private http: HttpClient) { }

  getBoard() {
    return this.http.get<Square[]>(this.boardApiUrl);
  }

  getMoves() {
    return this.http.get<Move[]>(this.moveApiUrl);
  }

  getRules() {
    return this.http.get<Rules>(this.boardApiUrl + "/rules");
  }


}
