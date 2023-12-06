export interface IColleagueModel {
  user:
    | {
        id: number | undefined;
        username: string | undefined;
        email: string | undefined;
      }
    | undefined;
  colleague:
    | {
        id: number | undefined;
        username: string | undefined;
        email: string | undefined;
      }
    | undefined;
}
