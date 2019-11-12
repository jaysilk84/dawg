import { TestBed } from '@angular/core/testing';

import { ScrabbleService } from './scrabble.service';

describe('ScrabbleService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: ScrabbleService = TestBed.get(ScrabbleService);
    expect(service).toBeTruthy();
  });
});
