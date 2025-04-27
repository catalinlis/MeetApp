import { ResolveFn } from '@angular/router';
import { InterestService } from '../_services/interest.service';
import { inject } from '@angular/core';

export const interestPlaceholderResolver: ResolveFn<string | null> = (route, state) => {
  const interestService = inject(InterestService);
  
  const interest = route.paramMap.get('interest');

  if(interest == null)
    return null;

  return interest;
};
