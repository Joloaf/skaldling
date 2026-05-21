<script lang="ts">
	import { spritesApi, type SpriteDto } from '$lib/api/sprites'
	import type { ProblemDetails } from '$lib/api/client';

	type GalleryState =
		| { status: 'idle' }
		| { status: 'loading' }
		| { status: 'loaded'; sprites: SpriteDto[] }
		| { status: 'error'; problem: ProblemDetails };

	let pageState: GalleryState = $state({ status: 'idle' });

	async function loadSprites() {
		pageState = { status: 'loading' };
		const result = await spritesApi.list();
		if (result.ok) {
			pageState = { status: 'loaded', sprites: result.data.sprites };
		} else {
			pageState = { status: 'error', problem: result.problem };
		}
	}
</script>

<h1>Skaldling - Sprite catalog</h1>
<button onclick={loadSprites}>Load sprites</button>

{#if pageState.status === 'loading'}
	<p>Loading...</p>
{:else if pageState.status === 'loaded'}
	<p>Loaded {pageState.sprites.length} sprite(s).</p>
	<ul style="list-style: none; padding: 0; display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: 1rem;">
		{#each pageState.sprites as sprite (sprite.id)}
			<li style="border: 1px solid #ddd; padding: 0.75rem; border-radius: 6px;">
				<img src={sprite.assetPath} alt={sprite.description} style="width: 100%; height: auto; background: #f6f1e8;" />
				<p style="margin: 0.5rem 0 0; font-weight: 600;">{sprite.name}</p>
				<p style="margin: 0.25rem 0 0; font-size: 0.85rem; color: #666;">{sprite.type} · layer {sprite.layer}</p>
				<p style="margin: 0.5rem 0 0; font-size: 0.85rem;">{sprite.description}</p>
			</li>
		{/each}
	</ul>
{:else if pageState.status === 'error'}
	<p style="color: crimson">
		Failed to load: {pageState.problem.title ?? 'unknown error'}
		{#if pageState.problem.status}({pageState.problem.status}){/if}
	</p>
{/if}