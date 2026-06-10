<script lang="ts">
	import { resolve } from '$app/paths';

	type Props = {
		heroName: string;
		earnedScore: number | null;
		lifetimePoints: number;
		heroId: string;
	};

	const { heroName, earnedScore, lifetimePoints, heroId }: Props = $props();
</script>

<section class="completion">
	<h2>Adventure complete</h2>
	{#if earnedScore !== null}
		<p style="font-size: 1.1rem;">
			{heroName} earned <strong>{earnedScore}</strong> achievement points on this adventure.
		</p>
	{/if}
	<p>Lifetime achievement points: <strong>{lifetimePoints}</strong></p>
	<p style="margin-top: 1.5rem;">
		<a href={resolve('/heroes/[id]/library', { id: heroId })}>See {heroName}'s adventure library</a>
	</p>
</section>

<style>
    .completion {
        max-width: 600px;
        margin: 1.5rem auto 0;
        padding: 2rem;
        background: linear-gradient(to bottom, #f0fff0, #e0ffe0);
        border: 2px solid forestgreen;
        border-radius: 8px;
        text-align: center;
        color: #1f3a23;
    }
    .completion h2 {
        margin-top: 0;
        color: forestgreen;
    }
</style>