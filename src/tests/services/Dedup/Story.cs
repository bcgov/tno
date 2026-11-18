namespace TNO.Test.Services.Dedup;

/// <summary>
/// Sample story text for duplicate detection tests.
/// </summary>
internal static class Story
{
    public const string Original =
        "The B.C. Conservatives have pledged to scrap the province's plan for a new tunnel under the Fraser River and build a bridge instead. " +
        "Party leader John Rustad made the announcement Thursday morning in Richmond, standing near the aging George Massey Tunnel. " +
        "He said a bridge would carry more traffic, cost less to maintain and could be finished sooner than the immersed tube tunnel the NDP government approved. " +
        "The current replacement project is estimated to cost more than four billion dollars and is not expected to open until the next decade. " +
        "Rustad said his government would review the existing contracts and begin consultation with Delta and Richmond within its first hundred days. " +
        "Commuters on Highway 99 have waited years for a replacement, and the crossing remains one of the most congested in the Lower Mainland. " +
        "The NDP said changing course now would add years of delay and waste the money already spent on design and early works. " +
        "The Greens said any new crossing should include rapid transit and protect the farmland on both sides of the river. " +
        "Local mayors have asked all parties to commit to a firm opening date and to keep the toll free crossing promise. " +
        "Voters head to the polls on Oct. 24, and advance voting has already drawn record numbers across the province. " +
        "The party also promised to widen the highway between the crossing and the border and to add a dedicated bus lane. " +
        "Engineers have warned that the tunnel is vulnerable in a major earthquake, which was one reason the original bridge plan was drawn up years ago.";

    /// <summary>
    /// The original with one sentence rewritten and a byline added, as publishers do when they republish a story.
    /// </summary>
    public const string Edited =
        "By staff, Black Press Media. " +
        "The B.C. Conservatives have pledged to scrap the province's plan for a new tunnel under the Fraser River and build a bridge instead. " +
        "Party leader John Rustad made the announcement Thursday morning in Richmond, standing near the aging George Massey Tunnel. " +
        "He said a bridge would carry more traffic, cost less to maintain and could be finished sooner than the immersed tube tunnel the NDP government approved. " +
        "The current replacement project is estimated to cost more than four billion dollars and is not expected to open until the next decade. " +
        "Rustad said his government would review the existing contracts and begin consultation with Delta and Richmond within its first hundred days. " +
        "Commuters on Highway 99 have waited years for a replacement, and the crossing remains one of the most congested in the Lower Mainland. " +
        "The NDP said changing course now would add years of delay and waste the money already spent on design and early works. " +
        "The Green Party said a crossing must include rapid transit and safeguard agricultural land on each side. " +
        "Local mayors have asked all parties to commit to a firm opening date and to keep the toll free crossing promise. " +
        "Voters head to the polls on Oct. 24, and advance voting has already drawn record numbers across the province. " +
        "The party also promised to widen the highway between the crossing and the border and to add a dedicated bus lane. " +
        "Engineers have warned that the tunnel is vulnerable in a major earthquake, which was one reason the original bridge plan was drawn up years ago.";

    public const string Unrelated =
        "Kelowna RCMP searched a home on Valley Road on Wednesday as part of an ongoing investigation into a series of break-ins. " +
        "Officers were seen carrying boxes from the residence late in the afternoon while neighbours watched from across the street. " +
        "Police said no one was arrested and there is no risk to the public, but they asked anyone with video from the area to come forward. " +
        "The detachment has reported a rise in property crime in the north end of the city over the past three months. " +
        "Residents said they had noticed unfamiliar vehicles parked on the street at night and had reported them to the non-emergency line. " +
        "A spokesperson said the investigation is in its early stages and more information will be released when it is appropriate. " +
        "Crime Stoppers is offering a reward for information that leads to an arrest in any of the break-ins. " +
        "The city recently added patrols and lighting in two parks where several thefts from vehicles were reported this summer.";
}
