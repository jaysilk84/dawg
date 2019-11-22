import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Square } from '../models/square.model';
import { Move } from '../models/move.model';
import { Rules } from '../models/rules.model';
import { Board } from '../models/board.model';
@Injectable({
  providedIn: 'root'
})
export class ScrabbleService {
  private readonly baseUrl = 'https://localhost:5001/';
  private readonly boardApiUrl = this.baseUrl + 'scrabble/board';
  private readonly moveApiUrl = this.baseUrl + 'scrabble/move';
  private readonly boardsApiUrl = this.baseUrl + 'scrabble/move/board';

  constructor(private http: HttpClient) { }

  getBoard() {
    return this.http.get<Square[][]>(this.boardApiUrl);
  }

  // getBoards() {
  //   return this.http.get<Board[]>(this.boardsApiUrl);
  // }

  postMove(board: Board) {
    return this.http.post<Board[]>(this.moveApiUrl, board);
  }

  getRules() {
    return this.http.get<Rules>(this.boardApiUrl + "/rules");
  }


}
