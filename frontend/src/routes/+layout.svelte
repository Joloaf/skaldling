<script lang="ts">
	import { resolve } from '$app/paths'
	import favicon from '$lib/assets/favicon.svg';

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

<nav style="padding: 1rem; border-bottom: 1px solid #ddd; display: flex; gap: 1rem;">
	<a href={resolve("/")}>Create hero</a>
	<a href={resolve("/heroes")}>Your heroes</a>
	<a href={resolve("/sprites")}>Sprites catalog</a>
</nav>

<main style="padding: 1rem;">
	{@render children()}
</main>
