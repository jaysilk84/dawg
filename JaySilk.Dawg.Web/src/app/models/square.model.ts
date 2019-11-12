import { Position } from './position.model';

export interface Square {
    position: Position;
    tile: string;
    isAnchor: boolean;
}