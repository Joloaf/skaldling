<script lang="ts">
	import { resolve } from '$app/paths';

	let { href = resolve('/'), label = 'Back to Menu' }: { href?: string; label?: string } = $props();
</script>

<a class="back-button" {href}>
	<span class="arrow" aria-hidden="true">←</span>
	{label}
</a>

<style>
    .back-button {
        display: inline-flex;
        align-items: center;
        gap: 0.4rem;
        margin-bottom: 1rem;
        color: var(--text-muted, #6b5d4a);
        text-decoration: none;
        font-size: 0.95rem;
    }
    .back-button:hover {
        color: var(--accent, #5b7a3a);
        text-decoration: underline;
    }
    .arrow {
        font-size: 1.1rem;
        line-height: 1;
    }
</style>