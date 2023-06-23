import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";

export const employeeSkillLevelApi = createApi({
    reducerPath: "employeeSkillLevelApi",
    baseQuery: fetchBaseQuery({
        baseUrl: "https://localhost:7100/api/",
    }),
    endpoints: (builder) => ({
        getAllEmployees: builder.query({
            query: () => `employees`,
        }),
    }),
});

export const {
    getAllEmployees,
} = employeeSkillLevelApi;