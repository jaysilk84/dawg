import { Square } from './square.model';
import { Move } from './move.model';

export interface Board {
    tiles: Square[];
    rack: string;
    playedWord: Move;
}