import { Position } from './position.model';

export interface Move {
    start: Position;
    end: Position;
    word: string;
    score: number;
    //blanks: number[];
}