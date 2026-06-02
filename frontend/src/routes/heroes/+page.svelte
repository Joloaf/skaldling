<script lang="ts">
	import { resolve } from '$app/paths';
	import { heroesApi, type HeroSummaryDto } from '$lib/api/heroes';
	import { spritesApi, type SpriteDto } from '$lib/api/sprites';
	import type { ProblemDetails } from '$lib/api/client';

	type PageState =
		| { status: 'loading' }
		| { status: 'loaded'; heroes: HeroSummaryDto[]; catalog: SpriteDto[] }
		| { status: 'error'; problem: ProblemDetails };

	let pageState: PageState = $state({ status: 'loading' });

	$effect(() => {
		loadEverything();
	});

	async function loadEverything() {
		pageState = { status: 'loading' };
		const [heroesResult, spritesResult] = await Promise.all([
			heroesApi.list(),
			spritesApi.list()
		]);
		if (!heroesResult.ok) {
			pageState = { status: 'error', problem: heroesResult.problem };
			return;
		}
		if (!spritesResult.ok) {
			pageState = { status: 'error', problem: spritesResult.problem };
			return;
		}
		pageState = {
			status: 'loaded',
			heroes: heroesResult.data.heroes,
			catalog: spritesResult.data.sprites
		};
	}

	function selectedInLayerOrder(avatarSpriteIds: string[], catalog: SpriteDto[]) {
		return avatarSpriteIds
			.map((id) => catalog.find((s) => s.id === id))
			.filter((s): s is SpriteDto => s !== undefined)
			.sort((a, b) => a.layer - b.layer);
	}
</script>

<h1>Your heroes</h1>

{#if pageState.status === 'loading'}
	<p>Loading…</p>
{:else if pageState.status === 'error'}
	<p style="color: crimson">
		Failed to load: {pageState.problem.title ?? 'unknown error'}
	</p>
{:else if pageState.status === 'loaded'}
	{#if pageState.heroes.length === 0}
		<p>No heroes yet. <a href={resolve("/")}>Create your first hero</a></p>
	{:else}
		<ul style="list-style: none; padding: 0; display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 1rem;">
			{#each pageState.heroes as hero (hero.id)}
				{@const editAvatarUrl = resolve('/heroes/[id]/avatar-builder', { id: hero.id })}
				{@const editDetailsUrl = resolve('/heroes/[id]/details', { id: hero.id })}
				{@const newAdventureUrl = resolve('/heroes/[id]/new-adventure', { id: hero.id })}
				<li style="border: 1px solid #ddd; padding: 0.75rem; border-radius: 6px;">
					<div style="position: relative; width: 128px; height: 128px; margin: 0 auto; background: #fafafa; border: 1px solid #eee;">
						{#each selectedInLayerOrder(hero.avatarSpriteIds, pageState.catalog) as sprite (sprite.id)}
							<img
								src={sprite.assetPath}
								alt={sprite.description}
								style="position: absolute; top: 0; left: 0; width: 100%; height: 100%;"
							/>
						{/each}
					</div>
					<p style="margin: 0.5rem 0 0; text-align: center; font-weight: 600;">{hero.name}</p>
					<p style="margin: 0.25rem 0 0; text-align: center; font-size: 0.85rem; color: #666;">
						Reading Age: {hero.readingAge}<br>Achievement Points: {hero.achievementPoints}
					</p>
					<p style="margin: 0.5rem 0 0; text-align: center;">
						<a href={editAvatarUrl}>Edit avatar</a> - <a href={editDetailsUrl}>Edit details</a>
					</p>
					<p style="margin: 0.25rem 0 0; text-align: center;">
						<a href={newAdventureUrl}>Start adventure</a>
					</p>
				</li>
			{/each}
		</ul>
		<p style="margin-top: 1rem;">
			<a href={resolve("/")}>Create another hero</a>
		</p>
	{/if}
{/if}