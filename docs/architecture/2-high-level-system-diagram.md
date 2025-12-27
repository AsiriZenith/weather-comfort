# High-Level System Diagram (Conceptual)

[ Browser (Angular) ]
|
| HTTPS + JWT
v
[ ASP.NET Core Web API ]
|
| Cached HTTP Calls
v
[ OpenWeatherMap API ]


Authentication is enforced at the API boundary using **Auth0-issued JWT tokens**.

---

