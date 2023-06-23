import { configureStore } from "@reduxjs/toolkit";
import { setupListeners } from "@reduxjs/toolkit/query";
import { employeeSkillLevelApi } from "./Services/employeeSkillLevel";

export const store = configureStore({
  reducer: {
    [employeeSkillLevelApi.reducerPath]: employeeSkillLevelApi.reducer,
  },
  // Adding the api middleware enables caching, invalidation, polling,
  // and other useful features of `rtk-query`.
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware()
      .concat(employeeSkillLevelApi.middleware)
});

setupListeners(store.dispatch);