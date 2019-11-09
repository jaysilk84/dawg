import { SimulationNodeDatum } from 'd3';


export interface Vertex extends SimulationNodeDatum {
    id: number;
    endOfWord: boolean;
    isRoot: boolean;
}
