import { CanActivateFn, RouteConfigLoadEnd, Router } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { inject } from '@angular/core';

export const registerGuardGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const router = inject(Router);

  if(accountService.currentUser()){

    const step = accountService.getRegisterStep();

    if(step == 1 && state.url !== "/register/image-upload"){
      router.navigate(["/register/image-upload"]);
      return false;
    }

    if(step == 2 && state.url !== "/register/choose-interest"){
      router.navigate(["/register/choose-interest"]);
      return false;
    }

    if(step == 3 && state.url !== "/profile"){
      router.navigate(["/profile"]);
      return false;
    }

    return true;
  }

  if(state.url !== '/register'){
    router.navigate(['/register']);
    return false;
  }

  return true;
};
